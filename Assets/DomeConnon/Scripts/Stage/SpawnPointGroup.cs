/*********************************************************************/
/**
 * @file   SpawnPointGroup.cs
 * @brief  円周上にSpawnPoint群（指定数）を自動生成・管理する
 *
 * Responsibility:
 * - 指定半径・個数に基づいて SpawnPoint Transform を生成する
 * - 高さモード（固定/パターン/波形量子化）に従いYを決定する
 * - 参照しやすい形（リスト）で外部へ提供する
 * - Scene上での配置確認用に Gizmo を描画する
 *
 * Notes:
 * - OnValidate 中に DestroyImmediate 系を呼ぶと Unity の制約で例外が出る場合がある
 *   → EditorApplication.delayCall で遅延実行して回避する
 */
/*********************************************************************/
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DomeCannon
{
    /// <summary>
    /// 円周上にSpawnPoint群を自動生成・管理する
    /// </summary>
    public sealed class SpawnPointGroup : MonoBehaviour
    {
        public enum HeightMode
        {
            Uniform,
            Pattern,
            QuantizedSine
        }

        [Header("Layout")]
        [SerializeField]
        private int spawnPointCount = 24;

        [SerializeField]
        private float radius = 10.0f;

        [Header("Height")]
        [SerializeField]
        private HeightMode heightMode = HeightMode.Uniform;

        [SerializeField]
        [Tooltip("床上面からのスポーン高さ[m]（Uniform用）")]
        private float uniformHeightFromFloor = 1.0f;

        [SerializeField]
        [Tooltip("Pattern用：床上面からの高さ[m]配列。SPに対して繰り返し適用される")]
        private float[] heightPatternFromFloor = new float[] { 1.0f, 1.5f, 2.0f, 1.5f };

        [SerializeField]
        [Tooltip("Pattern用：配列の適用開始インデックスをずらす")]
        private int patternOffset = 0;

        [SerializeField]
        [Tooltip("QuantizedSine用：基準高さ[m]")]
        private float sineBaseHeightFromFloor = 1.5f;

        [SerializeField]
        [Tooltip("QuantizedSine用：振幅[m]")]
        private float sineAmplitude = 1.0f;

        [SerializeField]
        [Tooltip("QuantizedSine用：円周あたりの波の回数")]
        private float sineCycles = 1.0f;

        [SerializeField]
        [Tooltip("QuantizedSine用：スナップ先の高さ[m]（例：Low/Mid/High）")]
        private float[] quantizeLevelsFromFloor = new float[] { 1.0f, 1.6f, 2.2f };

        [Header("Editor")]
        [SerializeField]
        private bool regenerateOnValidate = true;

        [SerializeField]
        private bool drawGizmos = true;

        /// <summary>
        /// 生成されたSpawnPointのリストを取得する
        /// </summary>
        public IReadOnlyList<Transform> Points => points;

        [SerializeField]
        private List<Transform> points = new List<Transform>(24);

        private const string SpawnPointNamePrefix = "SP_";

#if UNITY_EDITOR
        private bool regenerateQueued = false;
#endif

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!regenerateOnValidate)
            {
                return;
            }

            if (Application.isPlaying)
            {
                return;
            }

            QueueRegenerate();
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Editor上での再生成を遅延実行として予約する
        /// </summary>
        private void QueueRegenerate()
        {
            if (regenerateQueued)
            {
                return;
            }

            regenerateQueued = true;

            // Step 1: OnValidate中のDestroyImmediate禁止対策として次のEditor更新に回す
            EditorApplication.delayCall += ExecuteQueuedRegenerate;
        }

        private void ExecuteQueuedRegenerate()
        {
            regenerateQueued = false;

            if (this == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                return;
            }

            RegenerateNow();
        }
#endif

        /// <summary>
        /// SpawnPoint群を再生成する（実行環境に応じて処理を分岐）
        /// </summary>
        public void Regenerate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                QueueRegenerate();
                return;
            }
#endif
            RegenerateRuntimeSafe();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor上で即時再生成する（OnValidate外で呼ぶこと）
        /// </summary>
        [ContextMenu("Regenerate Now (Editor)")]
        public void RegenerateNow()
        {
            // Step 1: 既存ポイントをUndo対応で破棄
            ClearChildrenImmediateEditor();

            // Step 2: リスト初期化
            points.Clear();

            // Step 3: 入力値の最低限ガード
            if (spawnPointCount <= 0)
            {
                return;
            }

            float angleStepDeg = 360.0f / spawnPointCount;

            for (int i = 0; i < spawnPointCount; i++)
            {
                // Step 4: 円周上の位置
                float angleDeg = i * angleStepDeg;
                float rad = angleDeg * Mathf.Deg2Rad;

                float heightFromFloor = EvaluateHeightFromFloor(i, angleDeg);

                Vector3 localPos = new Vector3(
                    Mathf.Cos(rad) * radius,
                    heightFromFloor,
                    Mathf.Sin(rad) * radius
                );

                GameObject child = new GameObject($"{SpawnPointNamePrefix}{i:00}");
                Undo.RegisterCreatedObjectUndo(child, "Create SpawnPoint");

                child.transform.SetParent(transform, false);
                child.transform.localPosition = localPos;

                // Step 5: 内側（中心）を向ける
                Vector3 worldCenter = transform.position;
                child.transform.LookAt(worldCenter);

                points.Add(child.transform);
            }

            EditorUtility.SetDirty(this);
        }
#endif

        /// <summary>
        /// 再生中でも安全な再生成（DestroyImmediateを使わない）
        /// </summary>
        private void RegenerateRuntimeSafe()
        {
            // Step 1: 実行中に生成し直すケースは基本想定しない（必要になったらプール化する）
            // ここでは安全のため「既存を壊さずに整合性だけ取る」方針にする
            points.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                points.Add(transform.GetChild(i));
            }
        }

        private float EvaluateHeightFromFloor(int index, float angleDeg)
        {
            // Step 1: モード別に高さを決める
            switch (heightMode)
            {
                case HeightMode.Uniform:
                    return Mathf.Max(0.0f, uniformHeightFromFloor);

                case HeightMode.Pattern:
                    return Mathf.Max(0.0f, EvaluatePatternHeight(index));

                case HeightMode.QuantizedSine:
                    return Mathf.Max(0.0f, EvaluateQuantizedSineHeight(angleDeg));

                default:
                    return Mathf.Max(0.0f, uniformHeightFromFloor);
            }
        }

        private float EvaluatePatternHeight(int index)
        {
            // Step 1: 配列が無い場合はUniformへフォールバック
            if (heightPatternFromFloor == null || heightPatternFromFloor.Length == 0)
            {
                return uniformHeightFromFloor;
            }

            // Step 2: オフセット込みで繰り返し参照
            int offset = Mod(patternOffset, heightPatternFromFloor.Length);
            int idx = Mod(index + offset, heightPatternFromFloor.Length);
            return heightPatternFromFloor[idx];
        }

        private float EvaluateQuantizedSineHeight(float angleDeg)
        {
            // Step 1: サイン波で高さを生成
            float t = angleDeg / 360.0f;
            float phase = t * Mathf.PI * 2.0f * Mathf.Max(0.0f, sineCycles);
            float raw = sineBaseHeightFromFloor + Mathf.Sin(phase) * sineAmplitude;

            // Step 2: 量子化（近い段にスナップ）
            if (quantizeLevelsFromFloor == null || quantizeLevelsFromFloor.Length == 0)
            {
                return raw;
            }

            float best = quantizeLevelsFromFloor[0];
            float bestDist = Mathf.Abs(raw - best);

            for (int i = 1; i < quantizeLevelsFromFloor.Length; i++)
            {
                float v = quantizeLevelsFromFloor[i];
                float d = Mathf.Abs(raw - v);
                if (d < bestDist)
                {
                    best = v;
                    bestDist = d;
                }
            }

            return best;
        }

        private int Mod(int x, int m)
        {
            if (m <= 0)
            {
                return 0;
            }

            int r = x % m;
            return (r < 0) ? r + m : r;
        }

#if UNITY_EDITOR
        private void ClearChildrenImmediateEditor()
        {
            // Step 1: 子を逆順で削除（Undo対応）
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child != null)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }
#endif

        private void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, radius);

            for (int i = 0; i < points.Count; i++)
            {
                Transform p = points[i];
                if (p == null)
                {
                    continue;
                }

                Gizmos.DrawSphere(p.localPosition, 0.15f);
            }
        }
    }
}
