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
                break;

            case GameState.Gameplay:
                // Gameplay
                break;
            case GameState.ScoreCalc:
                CalculateScore();
                ChangeState(GameState.Result);
                break;
            case GameState.Result:
                // Result
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
}
