using System;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Effects;

namespace UnityJam.Environment
{
    /// <summary>
    /// SpawnPoint群から、宝箱/偽宝箱を指定数ランダムにスポーンする。
    /// - 重複なし（同じ地点に複数出さない）
    /// - Start時に一回だけ生成（毎フレInstantiateしない）
    /// - Find系は使わず、SpawnPointsRoot配下を列挙して収集する
    /// </summary>
    public sealed class TreasureSpawner : MonoBehaviour
    {
        [Header("--- Spawn Points ---")]
        [Tooltip("SpwanPointの親オブジェクトをバインド")]
        [SerializeField] private Transform spawnPointsRoot;

        [Header("--- Prefabs ---")]
        [Tooltip("宝箱のプレファブ")]
        [SerializeField] private GameObject treasurePrefab;
        [Tooltip("ミミックのプレファブ")]
        [SerializeField] private GameObject decoyPrefab;

        [Header("--- Counts ---")]
        [Tooltip("宝箱の数")]
        [Min(0)]
        [SerializeField] private int treasureCount = 5;

        [Tooltip("ミミックの数")]
        [Min(0)]
        [SerializeField] private int decoyCount = 3;

        [Header("Placement")]
        [Tooltip("Prefabのずれ")]
        [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0f, 0f);

        [Header("Post FX Injection (Optional)")]
        [Tooltip("宝箱開封時にBloomブーストさせたい場合、@PostFX_Global の BloomBurstController を設定")]
        [SerializeField] private BloomBurstController bloomBurst;

        [Header("Debug")]
        [Tooltip("ランダムを“完全ランダム”にするか“毎回同じ結果になるランダム”にするかを切り替える")]
        [SerializeField] private bool useDeterministicSeed = false;
        [Tooltip("決定的ランダムを使う場合のシード値")]
        [SerializeField] private int seed = 12345;

        private readonly List<Transform> spawnPoints = new List<Transform>(256);
        private readonly List<GameObject> spawnedObjects = new List<GameObject>(256);
        private bool spawnedOnce;

        private void Start()
        {
            Spawn();
        }

        /// <summary>
        /// 宝箱/偽宝箱をスポーンする（通常はStartで呼ぶ）。
        /// </summary>
        [ContextMenu("Spawn Now")]
        public void Spawn()
        {
            if (spawnedOnce)
            {
                return;
            }

            if (spawnPointsRoot == null)
            {
                Debug.LogWarning("TreasureSpawner: SpawnPointsRoot が未設定です。", this);
                return;
            }

            if (treasurePrefab == null)
            {
                Debug.LogWarning("TreasureSpawner: Treasure Prefab が未設定です。", this);
                return;
            }

            if (decoyPrefab == null)
            {
                Debug.LogWarning("TreasureSpawner: Decoy Prefab が未設定です。", this);
                return;
            }

            CollectSpawnPoints(spawnPointsRoot, spawnPoints);

            if (spawnPoints.Count == 0)
            {
                Debug.LogWarning("TreasureSpawner: SpawnPoint が 0 です。", this);
                return;
            }

            int totalAvailable = spawnPoints.Count;

            int tCount = Mathf.Clamp(treasureCount, 0, totalAvailable);
            int remainingAfterTreasure = totalAvailable - tCount;
            int dCount = Mathf.Clamp(decoyCount, 0, remainingAfterTreasure);

            if (tCount != treasureCount || dCount != decoyCount)
            {
                Debug.LogWarning(
                    $"TreasureSpawner: Count をクランプしました。treasure={treasureCount}->{tCount}, decoy={decoyCount}->{dCount} (SpawnPoints={totalAvailable})",
                    this);
            }

            // シャッフル（重複なし抽選のため、先頭から使用）
            // UnityJam.Environment 名前空間と System.Environment が衝突するため、
            // TickCount は必ず System.Environment.TickCount とフル修飾する。
            System.Random rng = useDeterministicSeed
                ? new System.Random(seed)
                : new System.Random(System.Environment.TickCount);

            ShuffleInPlace(spawnPoints, rng);

            // 先頭 tCount を宝箱、続く dCount を偽宝箱
            for (int i = 0; i < tCount; i++)
            {
                SpawnAt(treasurePrefab, spawnPoints[i]);
            }

            for (int i = 0; i < dCount; i++)
            {
                SpawnAt(decoyPrefab, spawnPoints[tCount + i]);
            }

            spawnedOnce = true;
        }

        /// <summary>
        /// SpawnPointsRoot配下から SpawnPoint を収集する。
        /// TreasureSpawnPoint コンポーネントが付いたものだけ対象。
        /// </summary>
        private static void CollectSpawnPoints(Transform root, List<Transform> outList)
        {
            outList.Clear();

            // Find系ではなく、階層を走査（Transform列挙）
            // GetComponentsInChildren でもよいが、ここは明示走査でGCを抑える。
            Stack<Transform> stack = new Stack<Transform>(128);
            stack.Push(root);

            while (stack.Count > 0)
            {
                Transform t = stack.Pop();

                // root自身はSpawnPointとして扱わない（子だけ想定）
                if (t != root)
                {
                    // マーカーが付いているものだけ採用
                    if (t.GetComponent<TreasureSpawnPoint>() != null)
                    {
                        outList.Add(t);
                    }
                }

                for (int i = 0; i < t.childCount; i++)
                {
                    stack.Push(t.GetChild(i));
                }
            }
        }

        private void SpawnAt(GameObject prefab, Transform point)
        {
            Vector3 pos = point.position + positionOffset;
            Quaternion rot = point.rotation;

            GameObject go = Instantiate(prefab, pos, rot, point);
            spawnedObjects.Add(go);

            // BloomBurst を自動注入（受け取れる側だけ）
            if (bloomBurst != null)
            {
                IBloomBurstReceiver receiver = go.GetComponent<IBloomBurstReceiver>();
                if (receiver != null)
                {
                    receiver.SetBloomBurst(bloomBurst);
                }
            }
        }


        /// <summary>
        /// リストをFisher-Yatesでインプレースシャッフル（LINQ不使用、GC抑制）。
        /// </summary>
        private static void ShuffleInPlace(List<Transform> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Transform tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        /// <summary>
        /// デバッグ用：生成済みを消して再スポーンできるようにする。
        /// （Play中の確認用。運用では多用しない）
        /// </summary>
        [ContextMenu("Clear Spawned")]
        public void ClearSpawned()
        {
            for (int i = spawnedObjects.Count - 1; i >= 0; i--)
            {
                if (spawnedObjects[i] != null)
                {
                    Destroy(spawnedObjects[i]);
                }
            }

            spawnedObjects.Clear();
            spawnedOnce = false;
        }
    }
}
