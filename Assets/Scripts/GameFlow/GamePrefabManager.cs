using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;

public class GamePrefabManager : MonoBehaviour
{
    public static GamePrefabManager Instance { get; private set; }

    [Header("State Game Prefabs")]
    [SerializeField] private List<GameObject> titlePrefabs;

    [SerializeField] private List<GameObject> gameplayPrefabs;
    [SerializeField] private List<GameObject> resultPrefabs;
    [SerializeField] private List<GameObject> gameOverPrefabs;

    private List<GameObject> currentPrefabInstances = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            // 初期状態反映
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        // 既存のUIを全て削除
        foreach (var instance in currentPrefabInstances)
        {
            if (instance != null) Destroy(instance);
        }
        currentPrefabInstances.Clear();

        List<GameObject> prefabsToInstantiate = null;

        switch (state)
        {
            case GameState.Title:
                prefabsToInstantiate = titlePrefabs;
                break;

            case GameState.Gameplay:
                prefabsToInstantiate = gameplayPrefabs;
                break;
            case GameState.Result:
                prefabsToInstantiate = resultPrefabs;
                break;
            case GameState.GameOver:
                prefabsToInstantiate = gameOverPrefabs;
                break;
                // 必要に応じて他のステートも追加
        }

        if (prefabsToInstantiate != null)
        {
            foreach (var prefab in prefabsToInstantiate)
            {
                if (prefab != null)
                {
                    float3 position = float3.zero;
                    Quaternion rotation = quaternion.identity;
                    // 親をこのManagerにする
                    GameObject uiObj = Instantiate(prefab, position, rotation, transform);
                    currentPrefabInstances.Add(uiObj);
                }
            }
        }
    }
}
