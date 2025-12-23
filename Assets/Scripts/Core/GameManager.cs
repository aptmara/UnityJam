using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Title,
    Select,
    StageIntro,
    Gameplay,
    ScoreCalc,
    Result,
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
                // Titleシーンへ (Single Scene: Title UI handling is done by UIManager)
                break;
            case GameState.Select:
                // セレクト画面・拠点へ
                break;
            case GameState.StageIntro:
                // ステージ開始演出。ステージ生成などはStageManagerが検知して行う
                break;
            case GameState.Gameplay:
                // プレイ開始処理（入力許可など）
                break;
            case GameState.ScoreCalc:
                // スコア計算してリザルトへ
                CalculateScore();
                ChangeState(GameState.Result);
                break;
            case GameState.Result:
                // 結果表示（UI側でハンドリング）
                break;
            case GameState.GameOver:
                // ゲームオーバー処理
                break;
        }
    }

    private void CalculateScore()
    {
        // PlayerDataManagerを使ってスコア計算
        Debug.Log("Calculating Score...");
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.AdvanceStage();
            Debug.Log($"Stage Advanced to: {PlayerDataManager.Instance.CurrentStageIndex}");
        }
    }
}
