using System;
using System.Collections;
using UnityEngine;


public enum GameState
{
    Title,
    Credits,
    Gameplay,
    ScoreCalc,
    Result,
    FinalResult,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    // イベント
    public event Action<GameState> OnStateChanged;

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
        // 初期状態はTitleと仮定
        ChangeState(GameState.Title);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        Debug.Log($"GameState Changing: {CurrentState} -> {newState}");

        CurrentState = newState;
        OnStateChanged?.Invoke(newState);

        HandleStateEnter(newState);
    }

    private void HandleStateEnter(GameState state)
    {
        switch (state)
        {
            case GameState.Title:
                // Title
                if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
                break;

            case GameState.Gameplay:
                // Gameplay
                break;
            case GameState.ScoreCalc:
                CalculateScore();
                ChangeState(GameState.Result);
                break;
            case GameState.Result:
                // Result (Round End)
                HandleRoundEnd();
                break;
            case GameState.FinalResult:
                // Final Result (Session End)
                break;
            case GameState.GameOver:
                // Game Over
                break;
        }
    }

    private void CalculateScore()
    {
        // PlayerDataManagerを使ってスコア計算
        Debug.Log("Calculating Score...");
    }

    /// <summary>
    /// ゴール到達時にStageManagerから呼ばれる
    /// </summary>
    public void HandleGoalReached()
    {
        if (UnityJam.Core.GameSessionManager.Instance != null && UnityJam.Core.GameSessionManager.Instance.CurrentFloor < UnityJam.Core.GameSessionManager.MaxFloors)
        {
            // 次の階層へ
            UnityJam.Core.GameSessionManager.Instance.ProceedToNextFloor();
            StartNextFloor();
        }
        else
        {
            // 1日の終了（5階層クリア）
            ChangeState(GameState.Result);
        }
    }

    private void StartNextFloor()
    {
        Debug.Log("Proceeding to Next Floor...");
        // 階層遷移時の演出（必要なら）
        // 現在はそのままリロード＝次の階層生成（StageManagerがCurrentFloorを見て生成を変える想定）
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
        {
             UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    private void HandleRoundEnd()
    {
        if (UnityJam.Core.Inventory.Instance != null && UnityJam.Core.GameSessionManager.Instance != null)
        {
            // 結果をセッションマネージャーに登録
            int score = UnityJam.Core.Inventory.Instance.TotalScore;
            var items = UnityJam.Core.Inventory.Instance.GetAllItems();
            
            // 1日の結果として登録
            UnityJam.Core.GameSessionManager.Instance.RegisterDayResult(score, items);

            // 3回(日)終わったかチェック
            if (UnityJam.Core.GameSessionManager.Instance.IsSessionFinished())
            {
                // 全ラウンド終了 -> 最終リザルトへ
                ChangeState(GameState.FinalResult);
            }
            else
            {
                // まだ続く -> 次のラウンド(日)へ
                Debug.Log("Day Finished. Proceeding to next day...");
            }
        }
    }

    public void StartNextRound()
    {
        // インベントリのクリア
        if (UnityJam.Core.Inventory.Instance != null)
        {
            UnityJam.Core.Inventory.Instance.Clear();
        }

        // プレイヤーやステージのリセット
        ChangeState(GameState.Gameplay);
        
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
        {
             UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
