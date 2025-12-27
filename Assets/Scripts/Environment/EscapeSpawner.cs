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

            System.Random rng = useDeterministicSeed
                ? new System.Random(seed)
                : new System.Random(System.Environment.TickCount); // ★ 修正点

            int index = rng.Next(0, spawnPoints.Count);
            Transform point = spawnPoints[index];

            Instantiate(exitPrefab, point.position + positionOffset, point.rotation);

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
