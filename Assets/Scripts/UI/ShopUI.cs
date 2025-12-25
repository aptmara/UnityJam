using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShopUI : MonoBehaviour
{

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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int BuyCost = nBatteryBuyCnt * BatteryNowCost;
        BatteryTMP.SetText("×{0} Cost:{1}", nBatteryBuyCnt,BuyCost);
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
        //プレイヤーの所持金を表示する
        HaveManeyText.SetText("$");
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
            LogText[LogEnd].SetText("Cant Buy Battery");
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
        LogText[LogEnd].SetText("Buy Battery");
        LogEnd++;
        BuyActionText.rectTransform.localPosition = new Vector3(-200, 70, 0);
        BuyActionFrame = 10;
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
        LogText[LogEnd].SetText("Cansel");
        LogEnd++;

        
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
        LogText[LogEnd].SetText("Confirm Buy. All Cost:{0}",AllBuyCost);

        nBatteryBuyCnt = 0;
        nLightBuyCnt = 0;

        LogEnd++;
    }

    public void OnShopEnd()
    {
        ScreenFader.Instance.FadeOut();
        //シーン読み込み

    }

    public void SetCost(int now,int future)
    {
        BatteryNowCost = now;
        BatteryNextCost = future;
    }
}
