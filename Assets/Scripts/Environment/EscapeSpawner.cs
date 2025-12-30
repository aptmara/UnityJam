using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityJam.Environment
{
    /// <summary>
    /// EscapeSpawnPoint 群から1つを選び、脱出地点Prefabを1つだけスポーンする。
    /// Start時に一度だけ実行（毎フレInstantiateしない）。
    /// </summary>
    public sealed class EscapeSpawner : MonoBehaviour
    {
        [Header("Spawn Points")]
        [Tooltip("EscapeSpawnPoint を子に持つ Transform")]
        [SerializeField] private Transform spawnPointsRoot;

        [Header("Spawn Count")]
        [Tooltip("生成する脱出地点の数")]
        [SerializeField] private int spawnCount = 2;

        [Header("Prefab")]
        [Tooltip("脱出地点のPrefab")]
        [SerializeField] private GameObject exitPrefab;

        [Header("Placement")]
        [Tooltip("脱出地点の位置オフセット")]
        [SerializeField] private Vector3 positionOffset = new Vector3(0f, 0.0f, 0f);

        [Header("Debug")]
        [Tooltip("決定論的なシードを使用するか")]
        [SerializeField] private bool useDeterministicSeed = false;
        [SerializeField] private int seed = 12345;

        private readonly List<Transform> spawnPoints = new List<Transform>(64);
        private bool spawnedOnce;

        private void Start()
        {
            Spawn();
        }

        [ContextMenu("Spawn Now")]
        public void Spawn()
        {
            if (spawnedOnce)
            {
                return;
            }

            if (spawnPointsRoot == null)
            {
                Debug.LogWarning("EscapeSpawner: SpawnPointsRoot が未設定です。", this);
                return;
            }

            if (exitPrefab == null)
            {
                Debug.LogWarning("EscapeSpawner: Exit Prefab が未設定です。", this);
                return;
            }

            CollectSpawnPoints(spawnPointsRoot, spawnPoints);

            if (spawnPoints.Count == 0)
            {
                Debug.LogWarning("EscapeSpawner: EscapeSpawnPoint が 0 です。", this);
                return;
            }

            if (spawnCount <= 0)
            {
                Debug.LogWarning("EscapeSpawner: spawnCount が 0 以下です。", this);
                return;
            }

            if (spawnCount > spawnPoints.Count)
            {
                Debug.LogWarning(
                    $"EscapeSpawner: spawnCount({spawnCount}) が SpawnPoint 数({spawnPoints.Count}) を超えています。全て使用します。",
                    this
                );
                spawnCount = spawnPoints.Count;
            }

            System.Random rng = useDeterministicSeed
                ? new System.Random(seed)
                : new System.Random(System.Environment.TickCount);

            // Fisher–Yates シャッフル
            for (int i = spawnPoints.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (spawnPoints[i], spawnPoints[j]) = (spawnPoints[j], spawnPoints[i]);
            }

            // 先頭から spawnCount 個生成
            for (int i = 0; i < spawnCount; i++)
            {
                Transform point = spawnPoints[i];
                // Parent to this spawner to ensure cleanup
                Instantiate(exitPrefab, point.position + positionOffset, point.rotation, transform);
            }

            spawnedOnce = true;
        }


        private static void CollectSpawnPoints(Transform root, List<Transform> outList)
        {
            outList.Clear();

            Stack<Transform> stack = new Stack<Transform>(64);
            stack.Push(root);

            while (stack.Count > 0)
            {
                Transform t = stack.Pop();

                if (t != root)
                {
                    if (t.GetComponent<EscapeSpawnPoint>() != null)
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
    }
}
