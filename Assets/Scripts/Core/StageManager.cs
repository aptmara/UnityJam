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

            if (GameManager.Instance.CurrentState == GameState.Gameplay)
            {
                SetupStage();
                if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
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
            case GameState.Title:
            case GameState.Gameplay:
                CleanupStage();
                SetupStage();
                if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
                break;
            case GameState.GameOver:
                // GameOver時は表示だけ残すかもしれないが、
                // タイトルに戻る時にCleanupされるのでここでは特に何もしない
                // 必要であればここでもCleanup
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

        // 3. プレイヤー生成
        if (currentPlayerInstance == null && playerPrefabs != null && playerPrefabs.Count > playerIndex)
        {
            GameObject prefab = playerPrefabs[playerIndex];
            if (prefab != null)
            {
                currentPlayerInstance = Instantiate(prefab, foundSpawnPoint.position, foundSpawnPoint.rotation);
            }
        }

        // 4. GameFlow初期化 (System内にGameFlowがあると仮定)
        if (currentSystemInstance != null)
        {
            var gameFlow = currentSystemInstance.GetComponentInChildren<UnityJam.Core.GameFlow>();
            if (gameFlow != null)
            {
                gameFlow.Initialize(currentStageInstance);
            }
        }


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



    private void CleanupStage()
    {
        if (currentPlayerInstance != null)
        {
            Destroy(currentPlayerInstance);
            currentPlayerInstance = null;
        }

        if (currentSystemInstance != null)
        {
            Destroy(currentSystemInstance);
            currentSystemInstance = null;
        }

        if (currentStageInstance != null)
        {
            Destroy(currentStageInstance);
            currentStageInstance = null;
        }
    }
}
