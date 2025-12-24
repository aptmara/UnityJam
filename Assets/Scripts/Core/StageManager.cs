using System.Collections.Generic;
using UnityEngine;
using UnityJam.Environment;

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
            case GameState.Result: // Add Result here to trigger cleanup and initial FadeIn
                CleanupStage();
                if (state == GameState.Gameplay) SetupStage(); // Only Setup for Gameplay
                
                // For Result, we just cleanup (GamePrefabManager handles UI). 
                // Then FadeIn.
                if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
                break;
            case GameState.GameOver:
                // GameOver時は表示だけ残すかもしれないが、
                // タイトルに戻る時にCleanupされるのでここでは特に何もしない
                // 必要であればここでもCleanup
                break;
        }
    }

    private void CleanupStage()
    {
        if (currentPlayerInstance != null) Destroy(currentPlayerInstance);
        if (currentSystemInstance != null) Destroy(currentSystemInstance);
        if (currentStageInstance != null) Destroy(currentStageInstance);

        currentPlayerInstance = null;
        currentSystemInstance = null;
        currentStageInstance = null;
    }

    private void SetupStage()
    {
        // インデックス取得 (PlayerDataManagerがあればそこから、なければ0)
        int stageIndex = PlayerDataManager.Instance ? PlayerDataManager.Instance.CurrentStageIndex : 0;
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
            Debug.LogWarning("GameManager.Instance is null - cannot change state.");
            return;
        }

        Debug.Log("GameManager.CurrentState = " + GameManager.Instance.CurrentState);

        if (currentPlayerInstance == null)
        {
            Debug.LogWarning("currentPlayerInstance is null - ignoring goal.");
            return;
        }

        // Ensure the triggered object is the current player or a child of it
        bool isCurrentPlayer = player == currentPlayerInstance || player.transform.IsChildOf(currentPlayerInstance.transform);
        Debug.Log("Is current player: " + isCurrentPlayer);
        if (!isCurrentPlayer)
        {
            return;
        }

        // Only consider hits to the designated Goal_G object (identity check handled in wrapper)
        if (currentGoalPoint == null)
        {
            Debug.LogWarning("currentGoalPoint is null - cannot verify goal identity.");
            return;
        }

        // Proceed to change state
        Debug.Log("Changing GameState to Result (triggered by goal: " + currentGoalPoint.gameObject.name + ")");
        
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOut(1.0f, () => 
            {
                GameManager.Instance.ChangeState(GameState.Result);
            });
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.Result);
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
