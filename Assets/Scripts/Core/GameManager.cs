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
    Shop,
    FinalResult,
    GameOver,
    Loading // Added for transitions
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    [Header("Day Display")]
    [SerializeField] private GameObject dayDisplayPrefab; // 日数表示用プレハブ（TMP含む）

    // イベント
    public event Action<GameState> OnStateChanged;

    // 重複呼び出し防止フラグ
    private bool isProcessingDayEnd = false;

    private void Awake()
    {
        Debug.Log($"[GameManager] Awake called on {gameObject.name} (ID: {GetInstanceID()}). Current Instance: {(Instance != null ? Instance.gameObject.name : "null")}");
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[GameManager] Set as Instance and DDOL. (ID: {GetInstanceID()})");
        }
        else
        {
            Debug.Log($"[GameManager] Instance already exists (ID: {Instance.GetInstanceID()}). Destroying this duplicate. (ID: {GetInstanceID()})");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[GameManager] OnDestroy called on {gameObject.name} (ID: {GetInstanceID()})");
        if (Instance == this)
        {
            Debug.LogWarning("[GameManager] Instance is being destroyed! This should only happen on application quit.");
             // Instance = null; // Usually Unity handles this, but good to know.
        }

        // 脱出イベント購読解除
        if (UnityJam.Core.EscapeState.Instance != null)
        {
            UnityJam.Core.EscapeState.Instance.OnEscaped -= HandleEscape;
        }
    }

    private void Start()
    {
        // 初期状態はTitleと仮定
        ChangeState(GameState.Title);

        // 脱出イベント購読
        if (UnityJam.Core.EscapeState.Instance != null)
        {
            UnityJam.Core.EscapeState.Instance.OnEscaped += HandleEscape;
        }
    }

    /// <summary>
    /// 脱出成功時の処理（スコアそのままでDayResultへ）
    /// </summary>
    private void HandleEscape()
    {
        // 重複防止
        if (isProcessingDayEnd)
        {
            Debug.LogWarning("[GameManager] HandleEscape called but already processing day end. Ignoring.");
            return;
        }
        isProcessingDayEnd = true;
        
        Debug.Log("Escape triggered! Transitioning to DayResult with current score.");

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOut(1.0f, () =>
            {
                // 脱出フラグリセット（次のラウンド用）
                if (UnityJam.Core.EscapeState.Instance != null)
                {
                    UnityJam.Core.EscapeState.Instance.ResetState();
                }
                // Resultへ遷移（HandleRoundEnd経由でスコア登録）
                ChangeState(GameState.Result);
            });
        }
        else
        {
            if (UnityJam.Core.EscapeState.Instance != null)
            {
                UnityJam.Core.EscapeState.Instance.ResetState();
            }
            ChangeState(GameState.Result);
        }
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
                // 新しい日の開始時にフラグリセット
                isProcessingDayEnd = false;
                
                // Gameplay - 日数表示付きフェードイン
                int currentDay = 1;
                if (UnityJam.Core.GameSessionManager.Instance != null)
                {
                    currentDay = UnityJam.Core.GameSessionManager.Instance.CurrentDayIndex + 1;
                }
                if (ScreenFader.Instance != null) 
                {
                    ScreenFader.Instance.FadeInWithDayText(currentDay, dayDisplayPrefab, 1f, 2f);
                }
                break;
            case GameState.ScoreCalc:
                CalculateScore();
                ChangeState(GameState.Result);
                break;
            case GameState.Result:
                // Result (Day End) - 3日目終了なら直接FinalResultへ
                HandleRoundEnd(directToFinal: false);
                if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
                break;
            case GameState.Shop:
                // Shop Entry
                Debug.Log("Entered Shop State");
                 if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
                break;
            case GameState.FinalResult:
                // Final Result (Session End)
                HandleRoundEnd(directToFinal: true);
                if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();
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
            // 重複防止
            if (isProcessingDayEnd)
            {
                Debug.LogWarning("[GameManager] HandleGoalReached (day end) called but already processing. Ignoring.");
                return;
            }
            isProcessingDayEnd = true;
            
            // 1日の終了（5階層クリア）
            ChangeState(GameState.Result);
        }
    }

    private void StartNextFloor()
    {
        Debug.Log("Proceeding to Next Floor...");
        StartCoroutine(LoadNextFloorRoutine());
    }

    private IEnumerator LoadNextFloorRoutine()
    {
        // 1. Loadingへ遷移（これでStageManager等のGameplayPrefabが破棄される）
        ChangeState(GameState.Loading);
        
        // 2. シーンリロード
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
        {
             // Asyncでロードせずとも、1フレーム待つなどでタイミングをずらす
             var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
             while (!op.isDone) yield return null;
        }
        else
        {
            // 仮にMainじゃない場合でも1フレーム待つ
            yield return null;
        }

        // 3. Gameplayへ復帰（StageManagerが新規生成され、Start()でステージ構築＆FadeInが走る）
        ChangeState(GameState.Gameplay);
    }

    private void HandleRoundEnd(bool directToFinal)
    {
        if (UnityJam.Core.Inventory.Instance != null && UnityJam.Core.GameSessionManager.Instance != null)
        {
            // FinalResultから呼ばれた場合は登録済みなのでスキップ
            if (!directToFinal)
            {
                // 結果をセッションマネージャーに登録
                int score = UnityJam.Core.Inventory.Instance.TotalScore;
                var items = UnityJam.Core.Inventory.Instance.GetAllItems();
                
                // 1日の結果として登録
                UnityJam.Core.GameSessionManager.Instance.RegisterDayResult(score, items);

                // 3日目終了なら直接FinalResultへ
                if (UnityJam.Core.GameSessionManager.Instance.IsSessionFinished())
                {
                    ChangeState(GameState.FinalResult);
                }
            }
            // directToFinal=trueの場合、FinalResultUI表示のみ（登録は済んでいる）
        }
    }

    /// <summary>
    /// 敵に食べられたりして失敗した場合の処理
    /// </summary>
    public void HandleDayFailed()
    {
        // 重複防止
        if (isProcessingDayEnd)
        {
            Debug.LogWarning("[GameManager] HandleDayFailed called but already processing day end. Ignoring.");
            return;
        }
        isProcessingDayEnd = true;
        
        Debug.Log("Day Failed! Penalty applied.");

        if (UnityJam.Core.Inventory.Instance != null && UnityJam.Core.GameSessionManager.Instance != null)
        {
            // インベントリ全没収
            UnityJam.Core.Inventory.Instance.Clear();

            // スコア0、アイテムなしで登録
            UnityJam.Core.GameSessionManager.Instance.RegisterDayResult(0, new System.Collections.Generic.Dictionary<UnityJam.Items.ItemMaster, int>());

            // 3日目終了なら直接FinalResultへ、そうでなければResultへ
            if (UnityJam.Core.GameSessionManager.Instance.IsSessionFinished())
            {
                ChangeState(GameState.FinalResult);
            }
            else
            {
                ChangeState(GameState.Result);
            }
        }
    }


    /// <summary>
    /// 次の日(Day)を開始する
    /// </summary>
    public void StartNextDay()
    {
        Debug.Log("Starting Next Day...");

        // セッションマネージャーの開始処理（階層設定など）
        if (UnityJam.Core.GameSessionManager.Instance != null)
        {
            UnityJam.Core.GameSessionManager.Instance.StartNextDay();
        }

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
