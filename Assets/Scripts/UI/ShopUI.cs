using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    // 静的コスト管理（セッション間で保持）
    private static int s_BatteryNowCost = 10;
    private static int s_BatteryNextCost = 15;
    private static float s_CostMultiplier = 1.5f;

    // 確定済みコスト（購入確定後のコスト、キャンセル時に戻る基準）
    private static int s_CommittedNowCost = 10;
    private static int s_CommittedNextCost = 15;

    /// <summary>
    /// コストを初期値にリセット（ゲームリセット時に呼ぶ）
    /// </summary>
    public static void ResetCost()
    {
        s_BatteryNowCost = 10;
        s_BatteryNextCost = 15;
        s_CommittedNowCost = 10;
        s_CommittedNextCost = 15;
        Debug.Log("[ShopUI] Cost Reset to initial values");
    }

    [Header("Audio")]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip BuySE;

    [Header("Buttons")]
    [SerializeField] Button batteryBuyButton;
    [SerializeField] Button cancelButton;
    [SerializeField] Button buyButton;
    [SerializeField] Button shopEndButton;

    [Header("BuyLog")]
    [SerializeField] TMPro.TMP_Text[] LogText;
    [SerializeField] TMPro.TMP_Text BuyActionText;
    [SerializeField] TMPro.TMP_Text HaveManeyText;

    int BuyActionFrame;

    [Header("BatteryBuyText")]
    [SerializeField] TMPro.TMP_Text BatteryTMP;
    [SerializeField] int MaxBuyBattery = 10;
    int LogEnd;
    [SerializeField] TMPro.TMP_Text BatteryNowCostTMP;
    [SerializeField] TMPro.TMP_Text BatteryNextCostTMP;

    int nBatteryBuyCnt;
    int nBatteryCartCost;

    int AllBuyCost;

    void OnEnable()
    {
        Debug.Log("[ShopUI] OnEnable called");

        // ショップ入店時にコストと購入済み追加バッテリー数をリセット（その日限りの強化とするため）
        ResetCost();
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.BatteryAdditionPieces = 0;
            Debug.Log("[ShopUI] Reset BatteryAdditionPieces to 0.");
        }

        for (int i = 0; i < 4; ++i)
        {
            if (LogText != null && i < LogText.Length && LogText[i] != null)
                LogText[i].SetText("");
        }
        LogEnd = 0;

        if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();

        // ショップ入店時のコストを確定コストとして記録（ResetCost後の値を採用）
        s_CommittedNowCost = s_BatteryNowCost;
        s_CommittedNextCost = s_BatteryNextCost;

        RefreshCostLabels();

        // 整合性チェックと復旧
        RestoreMissingScore();

        int score = GetTotalScore();
        Debug.Log($"[ShopUI] Initial Score: {score}, BatteryNowCost: {s_BatteryNowCost}, BatteryNextCost: {s_BatteryNextCost}");

        if (HaveManeyText != null)
        {
            HaveManeyText.text = score.ToString();
        }
    }

    void Start()
    {
        // ボタンイベント登録
        if (batteryBuyButton != null) batteryBuyButton.onClick.AddListener(OnBatteryBuy);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCansel);
        if (buyButton != null) buyButton.onClick.AddListener(OnBuy);
        if (shopEndButton != null) shopEndButton.onClick.AddListener(OnShopEnd);
    }

    void Update()
    {
        if (BatteryTMP != null)
        {
            BatteryTMP.text = $"×{nBatteryBuyCnt} Cost:{nBatteryCartCost}";
        }

        RefreshCostLabels();

        if (BuyActionText != null)
        {
            if (BuyActionFrame <= 0)
            {
                BuyActionText.rectTransform.localPosition = new Vector3(0, 1000, 0);
            }
            else
            {
                --BuyActionFrame;
            }
        }

        if (HaveManeyText != null)
        {
            // Use local GetTotalScore() to ensure consistency and fallback support
            HaveManeyText.text = GetTotalScore().ToString();
        }
    }

    void RefreshCostLabels()
    {
        if (BatteryNowCostTMP != null)
        {
            BatteryNowCostTMP.text = s_BatteryNowCost.ToString();
        }

        if (BatteryNextCostTMP != null)
        {
            BatteryNextCostTMP.text = s_BatteryNextCost.ToString();
        }
    }

    private int GetTotalScore()
    {
        int score = 0;
        if (UnityJam.Core.GameSessionManager.Instance != null)
        {
            // セッションマネージャーから全日程の合計スコアを取得（これが残高）
            score = UnityJam.Core.GameSessionManager.Instance.GetTotalScore();
        }

        // Fallback: If GSM score is 0 but Inventory has score (e.g. sync issue), use Inventory
        if (score == 0 && UnityJam.Core.Inventory.Instance != null && UnityJam.Core.Inventory.Instance.TotalScore > 0)
        {
            return UnityJam.Core.Inventory.Instance.TotalScore;
        }

        if (score == 0 && UnityJam.Core.Inventory.Instance != null)
        {
            return UnityJam.Core.Inventory.Instance.TotalScore;
        }

        return score;
    }

    public void OnBatteryBuy()
    {
        Debug.Log("[ShopUI] OnBatteryBuy clicked");

        if (MaxBuyBattery <= nBatteryBuyCnt)
        {
            AddLog("バッテリーはこれ以上買えません");
            return;
        }

        // カートに現在のコストで追加
        nBatteryCartCost += s_BatteryNowCost;
        nBatteryBuyCnt++;

        // 次回のコストを計算（表示用）
        s_BatteryNowCost = s_BatteryNextCost;
        s_BatteryNextCost = Mathf.RoundToInt(s_BatteryNowCost * s_CostMultiplier);

        AddLog($"バッテリーをカートに追加 (Cost: {nBatteryCartCost})");

        if (BuyActionText != null)
            BuyActionText.rectTransform.localPosition = new Vector3(-200, 70, 0);
        BuyActionFrame = 10;

        if (audioSource != null && BuySE != null)
            audioSource.PlayOneShot(BuySE);
    }

    public void OnCansel()
    {
        Debug.Log("[ShopUI] OnCansel clicked");
        nBatteryBuyCnt = 0;
        nBatteryCartCost = 0;

        // コストをショップ入店時（または最後の購入確定時）の値に戻す
        s_BatteryNowCost = s_CommittedNowCost;
        s_BatteryNextCost = s_CommittedNextCost;

        AddLog("購入をキャンセルしました");
    }

    public void OnBuy()
    {
        Debug.Log("[ShopUI] OnBuy clicked");

        AllBuyCost = nBatteryCartCost;

        Debug.Log($"[ShopUI] OnBuy: AllBuyCost={AllBuyCost}, BatteryCart={nBatteryCartCost}");

        if (AllBuyCost <= 0)
        {
            AddLog("購入するアイテムがありません");
            return;
        }

        // 所持金（全合計スコア）チェック
        int currentScore = GetTotalScore();
        if (currentScore < AllBuyCost)
        {
            AddLog("スコアが足りません！");
            return;
        }

        if (UnityJam.Core.GameSessionManager.Instance != null)
        {
            // 全合計から消費（新しい日から順に）
            bool success = UnityJam.Core.GameSessionManager.Instance.SpendTotalScore(AllBuyCost);
            if (!success)
            {
                AddLog("スコアが足りません！");
                return;
            }
        }
        else
        {
            AddLog("スコアデータがありません");
            return;
        }

        AddLog($"購入確定しました. 総費用:{AllBuyCost}");

        if (nBatteryBuyCnt > 0)
        {
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.BatteryAdditionPieces += nBatteryBuyCnt;
                Debug.Log($"[ShopUI] Added {nBatteryBuyCnt} batteries. Total: {PlayerDataManager.Instance.BatteryAdditionPieces}");
            }
            else
            {
                Debug.LogError("[ShopUI] PlayerDataManager.Instance is NULL! Cannot save battery purchase.");
            }
        }

        if (nBatteryBuyCnt > 0 && audioSource != null && BuySE != null)
            audioSource.PlayOneShot(BuySE);

        // 購入確定：現在のコストを確定コストとして記録
        s_CommittedNowCost = s_BatteryNowCost;
        s_CommittedNextCost = s_BatteryNextCost;

        nBatteryBuyCnt = 0;
        nBatteryCartCost = 0;
    }

    private void AddLog(string message)
    {
        if (LogText == null || LogText.Length == 0) return;

        if (LogEnd >= LogText.Length)
        {
            LogEnd = LogText.Length - 1;
            for (int i = 0; i < LogText.Length - 1; ++i)
            {
                if (LogText[i] != null && LogText[i + 1] != null)
                    LogText[i].SetText(LogText[i + 1].GetParsedText());
            }
        }

        if (LogText[LogEnd] != null)
            LogText[LogEnd].SetText(message);
        LogEnd++;
    }

    public void OnShopEnd()
    {
        Debug.Log("[ShopUI] OnShopEnd clicked");
        ScreenFader.Instance.FadeOut(1.0f, () =>
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNextDay();
            }
        });
    }

    public void SetCost(int now, int future)
    {
        s_BatteryNowCost = now;
        s_BatteryNextCost = future;
        RefreshCostLabels();
    }

    /// <summary>
    /// GSMのスコアが消失している場合、インベントリから復旧を試みる
    /// </summary>
    private void RestoreMissingScore()
    {
        if (UnityJam.Core.GameSessionManager.Instance != null && UnityJam.Core.Inventory.Instance != null)
        {
            var gsm = UnityJam.Core.GameSessionManager.Instance;
            var inv = UnityJam.Core.Inventory.Instance;

            // GSMスコアが0で、インベントリにスコアがある場合
            if (gsm.GetTotalScore() == 0 && inv.TotalScore > 0)
            {
                Debug.LogWarning($"[ShopUI] GSM Score is 0 but Inventory has {inv.TotalScore}. Attempting to restore.");

                // Case 1: GSMにデータが全くない（登録漏れ）
                if (gsm.DayScores.Count == 0)
                {
                    Debug.LogWarning("[ShopUI] Restoring score to GSM DayResult (Registering new).");
                    gsm.RegisterDayResult(inv.TotalScore, inv.GetAllItems());
                }
                // Case 2: GSMにデータはあるが0点（誤って0点が登録された、あるいはリセット不全）
                else if (gsm.DayScores.Count > 0)
                {
                    Debug.LogWarning($"[ShopUI] GSM has {gsm.DayScores.Count} entries but total is 0. Overwriting last entry with Inventory score.");
                    // 最新のレコードを上書きする
                    int lastIndex = gsm.DayScores.Count - 1;
                    gsm.DayScores[lastIndex] = inv.TotalScore;

                    // アイテムも同期しておく（念のため）
                    foreach (var kvp in inv.GetAllItems())
                    {
                        if (gsm.TotalItems.ContainsKey(kvp.Key))
                            gsm.TotalItems[kvp.Key] += kvp.Value; // 加算か上書きか微妙だが、0なら加算でOK
                        else
                            gsm.TotalItems[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
    }
}
