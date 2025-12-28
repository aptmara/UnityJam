using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShopUI : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]    AudioSource audioSource;
    [SerializeField]    AudioClip selectSE;
    [SerializeField]    AudioClip BuySE;

    [Header("BuyLog")]
    [SerializeField]
    TMPro.TMP_Text[] LogText;
    [SerializeField]
    TMPro.TMP_Text BuyActionText;
    [SerializeField]
    TMPro.TMP_Text HaveManeyText;

    int BuyActionFrame;

    [Header("BatteryBuyText")]
    [SerializeField]
    TMPro.TMP_Text BatteryTMP;
    [SerializeField, Tooltip("BatteryCost")]
    int BatteryNowCost = 100;
    int BatteryNextCost = 110;
    [SerializeField]
    int MaxBuyBattery = 0;
    int LogEnd;
    [SerializeField]
    TMPro.TMP_Text BatteryNowCostTMP;
    [SerializeField]
    TMPro.TMP_Text BatteryNextCostTMP;

    int nBatteryBuyCnt;

    [Header("LightBuyText")]
    //[SerializeField]
    //TMPro.TMP_Text LightTMP;
    [SerializeField, Tooltip("LightCost")]
    int LightCost = 200;
    [SerializeField]
    int MaxUpgrade = 0;

    int nLightBuyCnt;

    [Header("Shortcut")]
    [SerializeField] private TMPro.TMP_Text shortcutInfoText; // "スキップ不可" or "3Fまでスキップ(200G)"
    [SerializeField] private UnityEngine.UI.Button shortcutButton;
    private int shortcutCostPerFloor = 200;

    int AllBuyCost;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 4; ++i)
        {
            LogText[i].SetText("");
        }
        LogEnd = 0;
        BuyActionText = BuyActionText.GetComponent<TMPro.TMP_Text>();
        ScreenFader.Instance.FadeIn();

        UpdateShortcutUI();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int BuyCost = nBatteryBuyCnt * BatteryNowCost;
        BatteryTMP.SetText("×{0} 費用:{1}", nBatteryBuyCnt,BuyCost);
        BuyCost = nLightBuyCnt * LightCost;
        //LightTMP.SetText("×{0} Cost:{1}", nLightBuyCnt,BuyCost);

        BatteryNowCostTMP.SetText("{0}", BatteryNowCost);
        BatteryNextCostTMP.SetText("{0}", BatteryNextCost);

        if(BuyActionFrame <= 0)
        {
            BuyActionText.rectTransform.localPosition = new Vector3(0, 1000, 0);
        }
        else
        {
            --BuyActionFrame;
        }
        // Shortcut UI update (if dynamic) - moved to separate method or here if needed
        // For efficiency, mostly static unless updated.
        // UpdateShortcutUI(); // Call if needed in update loop, but cost is static per day.
        
        //プレイヤーの所持金を表示する
        HaveManeyText.SetText($"{GetTotalScore()}"); // Show actual score
    }

    // Helper to get score
    private int GetTotalScore()
    {
         if (UnityJam.Core.Inventory.Instance != null) return UnityJam.Core.Inventory.Instance.TotalScore; // Inventory uses current score?
         // Note: Inventory.TotalScore is reset day by day?
         // In new flow, RegisterDayResult clears inventory? No, clearing happens on StartNextDay.
         // So Inventory.TotalScore is available here.
         return UnityJam.Core.Inventory.Instance != null ? UnityJam.Core.Inventory.Instance.TotalScore : 0;
    }

    private void UpdateShortcutUI()
    {
        if (shortcutInfoText == null) return;
        
        if (UnityJam.Core.GameSessionManager.Instance != null)
        {
            int lastReached = UnityJam.Core.GameSessionManager.Instance.LastReachedFloor;
            // 固定コスト: 200スタート (Day2=200, Day3=250...)
            // CurrentDayIndexはShop突入前にインクリメントされている想定 (Day1終了後=1)
            int currentDayIndex = UnityJam.Core.GameSessionManager.Instance.CurrentDayIndex;
            // 補正: Day1(Idx0)終わり -> Idx1 (Day2) -> Cost 200
            // Day2(Idx1)終わり -> Idx2 (Day3) -> Cost 250
            int baseCost = 200;
            int cost = baseCost + (Mathf.Max(0, currentDayIndex - 1) * 50);

            if (lastReached > 1)
            {
                shortcutInfoText.text = $"{lastReached}Fから開始 (Cost:{cost})";
                
                if (shortcutButton != null)
                {
                    shortcutButton.interactable = true;
                }
            }
            else
            {
                shortcutInfoText.text = "スキップ不可";
                if (shortcutButton != null) shortcutButton.interactable = false;
            }
        }
    }
    
    public void OnShortcutBuy()
    {
        if (UnityJam.Core.GameSessionManager.Instance == null) return;
        if (UnityJam.Core.Inventory.Instance == null) return;

        int lastReached = UnityJam.Core.GameSessionManager.Instance.LastReachedFloor;
        if (lastReached <= 1) return;

        // Cost Calculation
        int currentDayIndex = UnityJam.Core.GameSessionManager.Instance.CurrentDayIndex;
        int baseCost = 200;
        int cost = baseCost + (Mathf.Max(0, currentDayIndex - 1) * 50);

        int currentScore = UnityJam.Core.Inventory.Instance.TotalScore;

        if (currentScore >= cost)
        {
            // Pay
            UnityJam.Core.Inventory.Instance.SpendScore(cost);
            
            // Apply Skip
            UnityJam.Core.GameSessionManager.Instance.NextDayStartFloor = lastReached;
            
            // Log
            if (LogEnd >= 4)
            {
                LogEnd = 3;
                for (int i = 0; i < 3; ++i)
                {
                    LogText[i].SetText(LogText[i + 1].GetParsedText());
                }
            }
            LogText[LogEnd].SetText($"Skipped to {lastReached}F (-{cost})");
            LogEnd++;
             
             // Update UI
             if (shortcutButton != null) shortcutButton.interactable = false; // bought
        }
        else
        {
             if (LogEnd >= 4)
             {
                 LogEnd = 3;
                 for (int i = 0; i < 3; ++i)
                 {
                     LogText[i].SetText(LogText[i + 1].GetParsedText());
                 }
             }
             LogText[LogEnd].SetText("Not enough Score");
             LogEnd++;
        }
    }
    
    public void OnBatteryBuy()
    {
        //プレイヤーを参照しつつ最大個数を上回っていたら
        if(MaxBuyBattery <= nBatteryBuyCnt)
        {
            if (LogEnd >= 4)
            {
                LogEnd = 3;
                for (int i = 0; i < 3; ++i)
                {
                    LogText[i].SetText(LogText[i + 1].GetParsedText());
                }
            }
            LogText[LogEnd].SetText("バッテリーはこれ以上買えません");
            LogEnd++;
            return;
        }
        nBatteryBuyCnt++;
        if (LogEnd >= 4)
        {
            LogEnd = 3;
            for (int i = 0; i < 3; ++i)
            {
                LogText[i].SetText(LogText[i + 1].GetParsedText()); 
            }
        }
        LogText[LogEnd].SetText("バッテリーを買いました");
        LogEnd++;
        BuyActionText.rectTransform.localPosition = new Vector3(-200, 70, 0);
        BuyActionFrame = 10;

        audioSource.PlayOneShot(selectSE);
    }

    public void OnLightBuy()
    {
        //プレイヤーを参照しつつ最大個数を上回っていたら
        if(MaxUpgrade <= nLightBuyCnt)
        {
            if (LogEnd >= 4)
            {
                LogEnd = 3;
                for (int i = 0; i < 3; ++i)
                {
                    LogText[i].SetText(LogText[i + 1].GetParsedText());
                }
            }
            LogText[LogEnd].SetText("Cant Buy Light");
            LogEnd++;
            return;
        }

        nLightBuyCnt++;
        if (LogEnd >= 4)
        {
            LogEnd = 3;
            for (int i = 0; i < 3; ++i)
            {
                LogText[i].SetText(LogText[i + 1].GetParsedText());
            }
        }
        LogText[LogEnd].SetText("Buy Light");
        LogEnd++;
        BuyActionText.rectTransform.localPosition = new Vector3(-280, 200, 0);
        BuyActionFrame = 10;
    }

    public void OnCansel()
    {
        nBatteryBuyCnt = 0;
        nLightBuyCnt = 0;
        if (LogEnd >= 4)
        {
            LogEnd = 3;
            for (int i = 0; i < 3; ++i)
            {
                LogText[i].SetText(LogText[i + 1].GetParsedText());
            }
        }
        LogText[LogEnd].SetText("購入をキャンセルしました");
        LogEnd++;

        audioSource.PlayOneShot(selectSE);
    }

    public void OnBuy()
    {
        //プレイヤー情報を参照して追加バッテリーの個数を増やす
        //MaxBuyBatteryにも格納

        //プレイヤー情報を参照してライトの角度を強化(仮)
        //MaxUpgradeにも格納


        if (LogEnd >= 4)
        {
            LogEnd = 3;
            for (int i = 0; i < 3; ++i)
            {
                LogText[i].SetText(LogText[i + 1].GetParsedText());
            }
        }
        //お金減らす処理
        AllBuyCost = nBatteryBuyCnt * BatteryNowCost + nLightBuyCnt * LightCost;
        LogText[LogEnd].SetText("購入確定しました. 総費用:${0}",AllBuyCost);

        if(nBatteryBuyCnt > 0)
            audioSource.PlayOneShot(BuySE);
        else
            audioSource.PlayOneShot(selectSE);

        nBatteryBuyCnt = 0;
        nLightBuyCnt = 0;

        LogEnd++;

    }

    public void OnShopEnd()
    {
        ScreenFader.Instance.FadeOut(1.0f, () =>
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNextDay();
            }
        });
        audioSource.PlayOneShot(selectSE);
        //シーン読み込み
    }

    public void SetCost(int now,int future)
    {
        BatteryNowCost = now;
        BatteryNextCost = future;
    }
}
