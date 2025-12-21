/*********************************************************************/
/**
 * @file   BoundaryWallBuilder.cs
 * @brief  円形ステージ外周の壁Collider（BoxCollider）を自動生成する
 *
 * Responsibility:
 * - 指定半径の円周に沿って BoxCollider を複数生成し、簡易的な円形壁を構築する
 * - CharacterController が外へ出ないように物理的にブロックする
 *
 * Notes:
 * - OnValidate中にDestroyImmediateを直接呼ばない（delayCallで遅延実行）
 * - 壁は見た目不要。ColliderのみでOK
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
    /// 円形ステージ外周の壁Collider（BoxCollider）を自動生成する
    /// </summary>
    public sealed class BoundaryWallBuilder : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField]
        private int segmentCount = 24;

        [SerializeField]
        private float radius = 10.0f;

        [SerializeField]
        [Tooltip("壁の高さ[m]")]
        private float wallHeight = 2.0f;

        [SerializeField]
        [Tooltip("壁の厚み[m]（外側方向）")]
        private float wallThickness = 0.5f;

        [SerializeField]
        [Tooltip("床上面からの壁の下端高さ[m]")]
        private float baseHeightFromFloor = 0.0f;

        [Header("Editor")]
        [SerializeField]
        private bool rebuildOnValidate = true;

        [SerializeField]
        private bool drawGizmos = true;

#if UNITY_EDITOR
        private bool rebuildQueued = false;
#endif

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!rebuildOnValidate || Application.isPlaying)
            {
                return;
            }

            QueueRebuild();
        }

        private void QueueRebuild()
        {
            if (rebuildQueued)
            {
                return;
            }

            rebuildQueued = true;
            EditorApplication.delayCall += ExecuteQueuedRebuild;
        }

        private void ExecuteQueuedRebuild()
        {
            rebuildQueued = false;

            if (this == null || Application.isPlaying)
            {
                return;
            }

            RebuildNow();
        }

        [ContextMenu("Rebuild Now (Editor)")]
        public void RebuildNow()
        {
            // Step 1: 既存を削除（Undo対応）
            ClearChildrenImmediateEditor();

            // Step 2: セグメント生成
            int count = Mathf.Max(3, segmentCount);
            float stepDeg = 360.0f / count;

            // Step 3: 1セグメントあたりの弦長（Boxの横幅）を算出
            float stepRad = stepDeg * Mathf.Deg2Rad;
            float chordLength = 2.0f * radius * Mathf.Sin(stepRad * 0.5f);

            for (int i = 0; i < count; i++)
            {
                float angleDeg = i * stepDeg;
                float rad = angleDeg * Mathf.Deg2Rad;

                Vector3 pos = new Vector3(Mathf.Cos(rad) * radius, baseHeightFromFloor, Mathf.Sin(rad) * radius);

                GameObject wall = new GameObject($"Wall_{i:00}");
                Undo.RegisterCreatedObjectUndo(wall, "Create Boundary Wall");

                wall.transform.SetParent(transform, false);
                wall.transform.localPosition = pos;

                // Step 4: 円の接線方向に向けて配置（外周に沿う）
                // forwardを中心向きにしてから、Y回転で接線方向にする
                wall.transform.LookAt(transform.position);
                wall.transform.Rotate(0.0f, 90.0f, 0.0f);

                // Step 5: Collider設定（見た目不要）
                BoxCollider col = wall.AddComponent<BoxCollider>();
                col.size = new Vector3(chordLength, wallHeight, wallThickness);
                col.center = new Vector3(0.0f, wallHeight * 0.5f, wallThickness * 0.5f);

                // 任意：壁用レイヤがあるなら設定
                // wall.layer = LayerMask.NameToLayer("Stage");
            }

            EditorUtility.SetDirty(this);
        }

        private void ClearChildrenImmediateEditor()
        {
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
            Gizmos.DrawWireSphere(new Vector3(0.0f, baseHeightFromFloor, 0.0f), radius);
        }
    }
}
