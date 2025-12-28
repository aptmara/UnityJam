using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLight : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField,Tooltip("減衰デバッグ(F10)")]
    bool isDampingDebug = false;

    [Header("LightBattery")]
    [SerializeField,Tooltip("初期バッテリーのゲージ")]
    GameObject BatteryLife;
    [SerializeField,Tooltip("追加バッテリーのゲージ")]
    GameObject AdditionBatteryLife;

    [SerializeField,Tooltip("初期バッテリーのテキスト")]
    TMPro.TMP_Text BatteryPiecesText;
    [SerializeField,Tooltip("追加バッテリーのテキスト")]
    TMPro.TMP_Text AdditionPiecesText;


    private Image BatteryImage;//初期バッテリーの画像
    private Image AdditionBatteryImage;//追加バッテリーの画像

    [Header("LightInfo")]
    /// <summary>
    /// 初期バッテリーの個数,Inspecterで初期値設定可能
    /// </summary>
    [SerializeField, Tooltip("初期のバッテリーの個数")]
    int BatteryPieces;
    /// <summary>
    /// 追加バッテリーの個数
    /// </summary>
    int BatteryAdditionPieces;
    /// <summary>
    /// 一つのバッテリーの残量(0.0 ~ 1.0)
    /// </summary>
    float LightBattery;

    [SerializeField,Tooltip("ライトの強度")]
    float LightPower = 55.0f;
    [SerializeField,Tooltip("ライトの照射角度")]
    float LightAngle = 45.0f;

    [SerializeField,Tooltip("ライト")]
    Light[] light;

    [Header("LightDecrease")]
    [SerializeField,Tooltip("強化ライトの持続時間")]
    float PowerLifeTime = 30.0f;
    [SerializeField, Tooltip("通常ライトの持続時間")]
    float LifeTime = 60.0f;
    [SerializeField,Tooltip("減衰加速の値")]
    float LifeDampingAcceleration = 1.1f;
    [SerializeField, Tooltip("減衰加速の最大時間")]
    float DmpAccelTime = 2.0f;
    [SerializeField, Tooltip("点滅時の光減衰最低％")]
    float LightBlinkingLowest = 10.0f;
    [SerializeField, Tooltip("点滅時の光減衰最大％")]
    float LightBlinkingMax = 100.0f;
    [SerializeField, Tooltip("ライト点滅開始の残バッテリー量％")]
    float LightBlinkingPercentage;
    float DmpAccelNowTime;//減衰加速の残り時間


    int LightBlinkingInterval = 180;//ライト点滅のインターバル
    bool isLighting = true;
    bool isCollect = false;

    float damageAmount;

    // Start is called before the first frame update
    void Start()
    {
        //コンポーネントの取得
        BatteryImage = BatteryLife.GetComponent<Image>();
        AdditionBatteryImage = AdditionBatteryLife.GetComponent<Image>();

        BatteryAdditionPieces = 0;

        LightBattery = 1.0f;

        damageAmount = 0.0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDampingDebug && Input.GetKeyDown(KeyCode.F10))
            CollectedDebufItem();
        if (isCollect)
        {//偽アイテムをとった際の減衰加速時間を一定時間にする
            DmpAccelNowTime -= 1.0f / 60.0f;
            if(DmpAccelNowTime < 0.0f)
            {
                isCollect = false;
            }
        }
        ///ライトそのものの処理
        //光ってる状態にするか否か
        if (isLighting)
        {
            foreach(Light itLight in light)
            {
                itLight.innerSpotAngle = LightAngle;//角度設定
                itLight.intensity = LightPower;
            }



            if (isCollect)//偽アイテムをとっているフラグが立ってたら
                if (BatteryAdditionPieces > 0)
                    LightBattery -= 1.0f / (PowerLifeTime * 60.0f) * LifeDampingAcceleration;//加速減衰
                else
                    LightBattery -= 1.0f / (LifeTime * 60.0f) * LifeDampingAcceleration;//加速減衰

            else//偽アイテムとっていなかったら
               if (BatteryAdditionPieces > 0)
                LightBattery -= 1.0f / (PowerLifeTime * 60.0f);
               else
                LightBattery -= 1.0f / (LifeTime * 60.0f);//加速減衰

            if(damageAmount != 0.0f)
            {
                LightBattery -= damageAmount;
                damageAmount = 0.0f;
            }

            float NormalizeLightBlinkingPercentage = LightBlinkingPercentage / 100.0f;
            if (LightBattery <= 0.0f)
            {
                // 減少量を取る
                float ReducedLight = LightBattery;



                while (LightBattery <= 0.0f && isLighting)
                { 
                    if (BatteryAdditionPieces > 0)
                    {
                        BatteryAdditionPieces--;
                    }
                    else
                    {
                        --BatteryPieces;
                    }

                    if (BatteryPieces <= 0)
                    {
                        isLighting = false;

                        //ライトを点灯させないための処理
                        foreach (Light itLight in light)
                        {
                            itLight.intensity = 0.0f;
                        }

                    }
                    else
                    {

                        LightBattery += 1.0f;
                        
                    }
                }

            }
            else if (LightBattery <= NormalizeLightBlinkingPercentage && BatteryAdditionPieces == 0)
            {//点滅の処理

                Debug.Log(NormalizeLightBlinkingPercentage);

                float BlinkungDecrease = LightBlinkingMax - LightBlinkingLowest;


                float NormalizeBlinkungDecrease = BlinkungDecrease / 100.0f;
                float NormalizeBlinkungLowest = LightBlinkingLowest / 100.0f;
                float DecreaseRate = LightBattery / NormalizeLightBlinkingPercentage;

                foreach (Light itLight in light)
                {
                    itLight.intensity = Mathf.Sin(LightBlinkingInterval * (Mathf.PI / 180)) * LightPower * (NormalizeBlinkungDecrease * DecreaseRate + NormalizeBlinkungLowest);
                }

                LightBlinkingInterval -= 3;
                if (LightBlinkingInterval <= 0)
                    LightBlinkingInterval = 180;
            }
        }
        else
        {//ライトを点灯させないための処理
            foreach (Light itLight in light)
            {
                itLight.intensity = 0.0f;
            }
            LightBattery = 0.0f;
        }
        Vector3 scale = new Vector3(1.0f, LightBattery, 1.0f);//ゲージ計算用
        if(BatteryAdditionPieces > 0)
        {//追加ライトがある場合のゲージ処理
            AdditionBatteryImage.rectTransform.localScale = scale;
            AdditionBatteryImage.color = Color.HSVToRGB(LightBattery * 0.3f, 1.0f, 1.0f);
        }
        else
        {//追加ライトがない場合のゲージ処理
            AdditionBatteryImage.rectTransform.localScale = new Vector3 (1.0f, 0.0f, 1.0f);
            BatteryImage.color = Color.HSVToRGB(LightBattery * 0.3f, 1.0f, 1.0f);
            BatteryImage.rectTransform.localScale = scale;
        }
        //テキスト設定
        BatteryPiecesText.SetText("{0}×",BatteryPieces);
        AdditionPiecesText.SetText("{0}×",BatteryAdditionPieces);
    }

    /// <summary>
    /// 偽アイテムとった瞬間に減衰加速を有効にする関数
    /// </summary>
    public void CollectedDebufItem()
    {
        isCollect = true;
        DmpAccelNowTime = DmpAccelTime;
    }
    /// <summary>
    /// バッテリー追加関数
    /// </summary>
    /// <param name="AddPieces">追加するバッテリーの個数</param>
    public void AddBattery(int AddPieces)
    {
        BatteryAdditionPieces += AddPieces;
    }

    public void ReduceBatteryByPercent(float percent)
    {
        damageAmount += percent * 100.0f;

    }
}
