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
    
    // 日の失敗フラグ（ペナルティ適用用）
    private bool isDayFailed = false;

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

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        SubscribeToEscapeEvent();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        if (UnityJam.Core.EscapeState.Instance != null)
        {
            UnityJam.Core.EscapeState.Instance.OnEscaped -= HandleEscape;
        }
    }

    private void Start()
    {
        // 初期状態はTitleと仮定
        ChangeState(GameState.Title);
        // Startでも念のため購読試行（OnEnableで済んでいるはずだが）
        SubscribeToEscapeEvent();
    }
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] Scene Loaded: {scene.name}");
        SubscribeToEscapeEvent();
        CleanupAudioListeners();
        
        // Gameplay状態なら日数を表示
        if (CurrentState == GameState.Gameplay)
        {
             // シーンリロード後などはここを通る可能性がある
             // ただしHandleStateEnterで処理されている場合もあるので注意
        }
    }

    private void SubscribeToEscapeEvent()
    {
        if (UnityJam.Core.EscapeState.Instance != null)
        {
            // 一度解除して重複を防ぐ
            UnityJam.Core.EscapeState.Instance.OnEscaped -= HandleEscape;
            UnityJam.Core.EscapeState.Instance.OnEscaped += HandleEscape;
            Debug.Log("[GameManager] Subscribed to EscapeState.OnEscaped event.");
        }
    }

    private void CleanupAudioListeners()
    {
        var listeners = FindObjectsOfType<AudioListener>();
        if (listeners.Length > 1)
        {
            Debug.LogWarning($"Found {listeners.Length} AudioListeners. Keeping one and disabling others.");
            for (int i = 1; i < listeners.Length; i++)
            {
                Destroy(listeners[i]); // コンポーネントのみ削除、あるいはGameObjectごと？コンポーネントのみが無難
            }
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
        if (UnityJam.Core.GameSessionManager.Instance != null)
        {
            // FinalResultから呼ばれた場合は登録済みなのでスキップ
            if (!directToFinal)
            {
                int score;
                System.Collections.Generic.Dictionary<UnityJam.Items.ItemMaster, int> items;
                
                if (isDayFailed)
                {
                    // 失敗時はスコア0、アイテムなし
                    score = 0;
                    items = new System.Collections.Generic.Dictionary<UnityJam.Items.ItemMaster, int>();
                    isDayFailed = false; // フラグをリセット
                }
                else
                {
                    // 通常成功時は現在のスコアとアイテム
                    score = UnityJam.Core.Inventory.Instance != null ? UnityJam.Core.Inventory.Instance.TotalScore : 0;
                    items = UnityJam.Core.Inventory.Instance != null 
                        ? UnityJam.Core.Inventory.Instance.GetAllItems() 
                        : new System.Collections.Generic.Dictionary<UnityJam.Items.ItemMaster, int>();
                }
                
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
        isDayFailed = true; // ペナルティフラグを立てる
        
        Debug.Log("Day Failed! Penalty applied.");

        if (UnityJam.Core.Inventory.Instance != null)
        {
            // インベントリ全没収
            UnityJam.Core.Inventory.Instance.Clear();
        }

        // Result画面へ遷移（RegisterDayResultはHandleRoundEnd内で行う）
        // 3日目終了判定はHandleRoundEnd内で行われる
        ChangeState(GameState.Result);
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
