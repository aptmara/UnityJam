using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
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
    [SerializeField, Tooltip("バッテリー初期コスト")]
    int BatteryNowCost = 10;
    int BatteryNextCost = 15;
    [SerializeField] float CostMultiplier = 1.5f;
    [SerializeField] int MaxBuyBattery = 10;
    int LogEnd;
    [SerializeField] TMPro.TMP_Text BatteryNowCostTMP;
    [SerializeField] TMPro.TMP_Text BatteryNextCostTMP;

    int nBatteryBuyCnt;
    int nBatteryCartCost;

    int AllBuyCost;

    void Start()
    {
        Debug.Log("[ShopUI] Start called");
        
        // ボタンイベント登録
        if (batteryBuyButton != null) batteryBuyButton.onClick.AddListener(OnBatteryBuy);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCansel);
        if (buyButton != null) buyButton.onClick.AddListener(OnBuy);
        if (shopEndButton != null) shopEndButton.onClick.AddListener(OnShopEnd);
        
        for (int i = 0; i < 4; ++i)
        {
            if (LogText != null && i < LogText.Length && LogText[i] != null)
                LogText[i].SetText("");
        }
        LogEnd = 0;
        
        ScreenFader.Instance.FadeIn();

        // コスト初期化
        BatteryNowCost = 10;
        BatteryNextCost = Mathf.RoundToInt(BatteryNowCost * CostMultiplier);
        
        RefreshCostLabels();
        
        int score = GetTotalScore();
        Debug.Log($"[ShopUI] Initial Score: {score}, BatteryNowCost: {BatteryNowCost}, BatteryNextCost: {BatteryNextCost}");
        
        if (HaveManeyText != null)
        {
            HaveManeyText.text = score.ToString();
        }
    }

    void FixedUpdate()
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
            HaveManeyText.text = GetTotalScore().ToString();
        }
    }

    void RefreshCostLabels()
    {
        if (BatteryNowCostTMP != null)
        {
            BatteryNowCostTMP.text = BatteryNowCost.ToString();
        }

        if (BatteryNextCostTMP != null)
        {
            BatteryNextCostTMP.text = BatteryNextCost.ToString();
        }
    }

    private int GetTotalScore()
    {
        if (UnityJam.Core.GameSessionManager.Instance != null)
        {
            int dayIndex = UnityJam.Core.GameSessionManager.Instance.CurrentDayIndex;
            if (dayIndex > 0)
            {
                return UnityJam.Core.GameSessionManager.Instance.GetDayScore(dayIndex);
            }
        }
        
        return UnityJam.Core.Inventory.Instance != null ? UnityJam.Core.Inventory.Instance.TotalScore : 0;
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
        nBatteryCartCost += BatteryNowCost;
        nBatteryBuyCnt++;
        
        // 次回のコストを計算（表示用）
        BatteryNowCost = BatteryNextCost;
        BatteryNextCost = Mathf.RoundToInt(BatteryNowCost * CostMultiplier);
        
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
        
        // コストを初期値に戻す
        BatteryNowCost = 10;
        BatteryNextCost = Mathf.RoundToInt(BatteryNowCost * CostMultiplier);
        
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

        int currentScore = GetTotalScore();
        if (currentScore < AllBuyCost)
        {
            AddLog("スコアが足りません！");
            return;
        }

        int dayIndex = UnityJam.Core.GameSessionManager.Instance != null 
            ? UnityJam.Core.GameSessionManager.Instance.CurrentDayIndex 
            : 0;
        
        if (dayIndex > 0 && UnityJam.Core.GameSessionManager.Instance != null)
        {
            bool success = UnityJam.Core.GameSessionManager.Instance.SpendFromDayScore(dayIndex, AllBuyCost);
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
        BatteryNowCost = now;
        BatteryNextCost = future;
        RefreshCostLabels();
    }
}
