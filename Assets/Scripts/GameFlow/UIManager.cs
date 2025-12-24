using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("State UI Prefabs")]
    [SerializeField] private List<GameObject> titlePrefabs;

    [SerializeField] private List<GameObject> gameplayPrefabs;
    [SerializeField] private List<GameObject> resultPrefabs;
    [SerializeField] private List<GameObject> gameOverPrefabs;

    private List<GameObject> currentUIInstances = new List<GameObject>();

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
        foreach (var instance in currentUIInstances)
        {
            if (instance != null) Destroy(instance);
        }
        currentUIInstances.Clear();

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
            // アクティブなCanvasを探して親にする
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform parentTransform = canvas != null ? canvas.transform : transform;

            foreach (var prefab in prefabsToInstantiate)
            {
                if (prefab != null)
                {
                    // falseを指定してプレハブのローカル設定（位置・スケール）を維持
                    GameObject uiObj = Instantiate(prefab, parentTransform, false);
                    currentUIInstances.Add(uiObj);
                }
            }
        }
    }
}
