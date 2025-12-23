using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private List<GameObject> playerPrefabs;
    [SerializeField] private List<GameObject> stagePrefabs;
    [SerializeField] private List<GameObject> systemPrefabs;


    [Header("Stage Settings")]
    [SerializeField] private float stageTimeLimit = 300f;

    private GameObject currentStageInstance;
    private GameObject currentSystemInstance;
    private GameObject currentPlayerInstance;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;

            if (GameManager.Instance.CurrentState == GameState.StageIntro)
            {
                SetupStage();
            }
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
        switch (state)
        {
            case GameState.StageIntro:
                SetupStage();
                break;
            case GameState.Gameplay:
                // ゲームプレイ開始処理
                break;
            case GameState.GameOver:
                break;
        }
    }

    private void SetupStage()
    {
        // インデックス取得 (PlayerDataManagerがあればそこから、なければ0)
        int stageIndex = PlayerDataManager.Instance ? PlayerDataManager.Instance.CurrentStageIndex : 0;
        int playerIndex = PlayerDataManager.Instance ? PlayerDataManager.Instance.CurrentPlayerIndex : 0;

        // 1. ステージ生成
        if (currentStageInstance == null && stagePrefabs != null && stagePrefabs.Count > stageIndex)
        {
            GameObject prefab = stagePrefabs[stageIndex];
            if (prefab != null)
            {
                currentStageInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
        }

        // 2. システム生成 (ステージ固有のシステム)
        if (currentSystemInstance == null && systemPrefabs != null && systemPrefabs.Count > stageIndex)
        {
            GameObject prefab = systemPrefabs[stageIndex];
            if (prefab != null)
            {
                currentSystemInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
        }

        // 2. スポーンポイント検索
        Transform foundSpawnPoint = FindSpawnPointInStage(currentStageInstance);

        if (foundSpawnPoint == null)
        {
            Debug.LogError("SpawnPoint not found in Stage Prefab!");
            foundSpawnPoint = transform;
        }

        // 3. プレイヤー生成
        if (currentPlayerInstance == null && playerPrefabs != null && playerPrefabs.Count > playerIndex)
        {
            GameObject prefab = playerPrefabs[playerIndex];
            if (prefab != null)
            {
                currentPlayerInstance = Instantiate(prefab, foundSpawnPoint.position, foundSpawnPoint.rotation);
            }
        }

        StartGameplay();
    }

    private Transform FindSpawnPointInStage(GameObject stage)
    {
        if (stage == null) return null;

        // 簡易的な再帰検索
        Transform[] allChildren = stage.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name == "SpawnPoint")
            {
                return child;
            }
        }

        return null;
    }

    private void StartGameplay()
    {
        GameManager.Instance.ChangeState(GameState.Gameplay);
    }
}
