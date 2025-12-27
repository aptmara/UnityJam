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

    private void HandleRoundEnd()
    {
        if (UnityJam.Core.Inventory.Instance != null && UnityJam.Core.GameSessionManager.Instance != null)
        {
            // 結果をセッションマネージャーに登録
            int score = UnityJam.Core.Inventory.Instance.TotalScore;
            var items = UnityJam.Core.Inventory.Instance.GetAllItems();
            UnityJam.Core.GameSessionManager.Instance.RegisterRoundResult(score, items);

            // 3回終わったかチェック
            if (UnityJam.Core.GameSessionManager.Instance.IsSessionFinished())
            {
                // 全ラウンド終了 -> 最終リザルトへ
                ChangeState(GameState.FinalResult);
            }
            else
            {
                // まだ続く -> 次のラウンドへ (少し待ってから、あるいはUIでボタンを押してから)
                // ここではシンプルにログを出し、InventoryをクリアしてGameplayに戻る例を示す
                // 実際はResultUIで「Next Round」ボタンを押させるのが一般的
                Debug.Log("Round Finished. Proceeding to next round...");
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

        // プレイヤーやステージのリセット (シーンのリロードが手っ取り早い場合が多い)
        // ここではGameplay状態へ戻す
        ChangeState(GameState.Gameplay);
        
        // 注意: 実際にはステージ生成の再実行などが必要。
        // シーン遷移を伴う場合はSceneManager.LoadSceneを使うか、StageManagerにリセット処理を実装する。
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
        {
             UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}
