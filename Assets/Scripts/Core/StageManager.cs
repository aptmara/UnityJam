using System.Collections.Generic;
using UnityEngine;
using UnityJam.Environment;
using UnityJam.Core;

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
    private StartPoint currentStartPoint;
    private GoalPoint currentGoalPoint;
    private System.Action<GameObject> currentGoalReachedDelegate;

    private void Start()
    {
        // プレハブとして生成された瞬間(=Gameplay開始)に構築
        SetupStage();
        if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
    }

    private void OnDestroy()
    {
        CleanupStage();
    }

    private void SetupStage()
    {
        // インデックス取得
        // GameSessionManagerがあればそこから階層を取得(1始まりなので-1)
        int stageIndex = 0;
        if (GameSessionManager.Instance != null)
        {
            stageIndex = GameSessionManager.Instance.CurrentFloor - 1;
        }
        else if (PlayerDataManager.Instance != null)
        {
            stageIndex = PlayerDataManager.Instance.CurrentStageIndex;
        }

        int playerIndex = PlayerDataManager.Instance ? PlayerDataManager.Instance.CurrentPlayerIndex : 0;

        // 1. ステージ生成
        if (stagePrefabs != null && stagePrefabs.Count > 0)
        {
            GameObject prefab = stagePrefabs[stageIndex];
                if (prefab != null)
                {
                    currentStageInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                }
        }

        // 2. システム生成 (ステージ固有のシステム)
        if (systemPrefabs != null && systemPrefabs.Count > 0)
        {
            GameObject prefab = systemPrefabs[stageIndex];
                if (prefab != null)
                {
                    currentSystemInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
                }
        }

        // 2. スポーンポイント検索
        // 優先: 子オブジェクト名で Start_S / Goal_G を探す
        currentStartPoint = null;
        currentGoalPoint = null;
        Transform foundSpawnPoint = null;

        if (currentStageInstance != null)
        {
            Transform[] allChildren = currentStageInstance.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.name == "Start_S" && foundSpawnPoint == null)
                {
                    foundSpawnPoint = child;
                    currentStartPoint = child.GetComponent<StartPoint>();
                }

                if (child.name == "Goal_G" && currentGoalPoint == null)
                {
                    currentGoalPoint = child.GetComponent<GoalPoint>();
                }
            }
        }

        // フォールバック: StartPoint コンポーネントがあればそれを使用
        if (foundSpawnPoint == null && currentStageInstance != null)
        {
            var spComp = currentStageInstance.GetComponentInChildren<StartPoint>();
            if (spComp != null)
            {
                currentStartPoint = spComp;
                foundSpawnPoint = spComp.PointTransform;
            }
        }

        // フォールバック: 旧来の "SpawnPoint" 名で検索
        if (foundSpawnPoint == null)
        {
            foundSpawnPoint = FindSpawnPointInStage(currentStageInstance);
        }

        if (foundSpawnPoint == null)
        {
            Debug.LogError("SpawnPoint not found in Stage Prefab!");
            foundSpawnPoint = transform;
        }

        // フォールバック: GoalPoint コンポーネントを探す
        if (currentGoalPoint == null && currentStageInstance != null)
        {
            currentGoalPoint = currentStageInstance.GetComponentInChildren<GoalPoint>();
        }

        // 3. プレイヤー生成
        if (playerPrefabs != null && playerPrefabs.Count > playerIndex)
        {
            GameObject prefab = playerPrefabs[playerIndex];
                if (prefab != null)
                {
                    currentPlayerInstance = Instantiate(prefab, foundSpawnPoint.position, foundSpawnPoint.rotation, transform);
                }
        }

        // Subscribe to GoalPoint reached event (store delegate so we can unsubscribe)
        if (currentGoalPoint != null)
        {
            // create delegate capturing the specific goal instance so identity is preserved
            var goalRef = currentGoalPoint;
            currentGoalReachedDelegate = (player) => OnGoalReachedWithGoal(goalRef, player);
            currentGoalPoint.OnReached += currentGoalReachedDelegate;
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

    private void OnGoalReached(GameObject player)
    {
        Debug.Log("Goal Reached by " + player.name);

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null - cannot change state.");
            return;
        }

        Debug.Log("GameManager.CurrentState = " + GameManager.Instance.CurrentState);

        if (currentPlayerInstance == null)
        {
            Debug.LogError("currentPlayerInstance is null - ignoring goal. Player might have been destroyed?");
            return;
        }

        // Ensure the triggered object is the current player or a child of it
        bool isCurrentPlayer = player == currentPlayerInstance || player.transform.IsChildOf(currentPlayerInstance.transform);
        Debug.Log($"Is current player: {isCurrentPlayer} (Triggered: {player.name}, Registered: {currentPlayerInstance.name})");
        
        if (!isCurrentPlayer)
        {
            Debug.LogError("Goal triggerer is NOT the registered current player. Ignoring.");
            return;
        }

        // Only consider hits to the designated Goal_G object (identity check handled in wrapper)
        if (currentGoalPoint == null)
        {
            Debug.LogError("currentGoalPoint is null - cannot verify goal identity.");
            return;
        }

        // Proceed to change state
        Debug.Log("Goal Reached! notifying GameManager...");

        if (ScreenFader.Instance != null)
        {
            Debug.Log("Starting FadeOut...");
            ScreenFader.Instance.FadeOut(1.0f, () =>
            {
                Debug.Log("FadeOut Complete. Calling HandleGoalReached.");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.HandleGoalReached();
                }
                else
                {
                    Debug.LogError("GameManager lost during fade!");
                }
            });
        }
        else
        {
            Debug.Log("No ScreenFader found. Calling HandleGoalReached immediately.");
            GameManager.Instance.HandleGoalReached();
        }
    }

    private void OnGoalReachedWithGoal(GoalPoint goal, GameObject player)
    {
        Debug.Log("OnGoalReachedWithGoal called. goal==currentGoalPoint: " + (goal == currentGoalPoint) + ", goalName: " + (goal != null ? goal.gameObject.name : "null"));
        // Ensure we only respond to the specific goal instance we registered
        if (goal != currentGoalPoint)
        {
            Debug.Log("Received goal event for a different goal instance - ignoring.");
            return;
        }

        OnGoalReached(player);
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

        // Unsubscribe and clear start/goal references
        if (currentGoalPoint != null)
        {
            if (currentGoalReachedDelegate != null)
            {
                currentGoalPoint.OnReached -= currentGoalReachedDelegate;
                currentGoalReachedDelegate = null;
            }
            currentGoalPoint = null;
        }

        currentStartPoint = null;
    }
}
