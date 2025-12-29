using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerLight : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField, Tooltip("減衰デバッグ(F10)")]
    bool isDampingDebug = false;

    [Header("LightBattery")]
    [SerializeField, Tooltip("初期バッテリーのゲージ")]
    GameObject BatteryLife;
    [SerializeField, Tooltip("追加バッテリーのゲージ")]
    GameObject AdditionBatteryLife;

    [SerializeField, Tooltip("初期バッテリーのテキスト")]
    TMPro.TMP_Text BatteryPiecesText;
    [SerializeField, Tooltip("追加バッテリーのテキスト")]
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

    [SerializeField, Tooltip("ライトの強度")]
    float LightPower = 55.0f;
    [SerializeField, Tooltip("ライトの照射角度")]
    float LightAngle = 45.0f;

    [SerializeField, Tooltip("ライト")]
    Light[] light;

    [Header("LightDecrease")]
    [SerializeField, Tooltip("強化ライトの持続時間")]
    float PowerLifeTime = 30.0f;
    [SerializeField, Tooltip("通常ライトの持続時間")]
    float LifeTime = 60.0f;
    [SerializeField, Tooltip("減衰加速の値")]
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
        BatteryImage = BatteryLife != null ? BatteryLife.GetComponent<Image>() : null;
        AdditionBatteryImage = AdditionBatteryLife != null ? AdditionBatteryLife.GetComponent<Image>() : null;

        // 参照ミス/反転/レイアウト介入を検出して警告（原因を確定させる）
        ValidateGaugeSetup();

        // UVを切って減少表現にする（RectTransform縮小ではなくFilledで表現）
        SetupGaugeImage(BatteryImage);
        SetupGaugeImage(AdditionBatteryImage);

        // 永続化データのロード
        if (PlayerDataManager.Instance != null)
        {
            if (PlayerDataManager.Instance.BatteryPieces == -1)
            {
                // 初回（データなし）：現在のInspector値を保存
                PlayerDataManager.Instance.BatteryPieces = BatteryPieces;
                PlayerDataManager.Instance.BatteryAdditionPieces = BatteryAdditionPieces;
                PlayerDataManager.Instance.LightBattery = LightBattery <= 0.0f ? 1.0f : LightBattery;
            }
            else
            {
                // データあり：ロードして上書き
                BatteryPieces = PlayerDataManager.Instance.BatteryPieces;
                BatteryAdditionPieces = PlayerDataManager.Instance.BatteryAdditionPieces;
                LightBattery = PlayerDataManager.Instance.LightBattery;
            }
        }
        else
        {
            // 既存挙動を維持：DataManagerなしの場合は追加0で開始
            BatteryAdditionPieces = 0;
            if (LightBattery <= 0.0f) LightBattery = 1.0f;
        }

        // 初期のUI反映
        ApplyGaugeUI();
        ApplyTextUI();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDampingDebug && Input.GetKeyDown(KeyCode.F10))
            CollectedDebufItem();

        if (isCollect)
        {//偽アイテムをとった際の減衰加速時間を一定時間にする
            DmpAccelNowTime -= 1.0f / 60.0f;
            if (DmpAccelNowTime < 0.0f)
            {
                isCollect = false;
            }
        }

        ///ライトそのものの処理
        //光ってる状態にするか否か
        if (isLighting)
        {
            foreach (Light itLight in light)
            {
                itLight.innerSpotAngle = LightAngle;//角度設定
                itLight.intensity = LightPower;
            }

            // 追加バッテリーがある間は強化ライト（既存仕様を維持）
            bool hasAdditionBattery = (BatteryAdditionPieces > 0);

            // 減衰
            float baseDrainPerSec = hasAdditionBattery ? (1.0f / PowerLifeTime) : (1.0f / LifeTime);
            float drainPerFrame = baseDrainPerSec * (1.0f / 60.0f);

            if (isCollect)
            {
                drainPerFrame *= LifeDampingAcceleration;//加速減衰
            }

            LightBattery -= drainPerFrame;

            if (damageAmount != 0.0f)
            {
                LightBattery -= damageAmount;
                damageAmount = 0.0f;
            }

            float NormalizeLightBlinkingPercentage = LightBlinkingPercentage / 100.0f;

            if (LightBattery <= 0.0f)
            {
                // 0未満に落ちた分を繰り越しつつ、バッテリー消費（追加→初期の順）
                ConsumeBatteryWithCarry();
            }
            else if (LightBattery <= NormalizeLightBlinkingPercentage && BatteryAdditionPieces == 0)
            {//点滅の処理（追加バッテリーがない時のみ：既存仕様）
                float BlinkungDecrease = LightBlinkingMax - LightBlinkingLowest;

                float NormalizeBlinkungDecrease = BlinkungDecrease / 100.0f;
                float NormalizeBlinkungLowest = LightBlinkingLowest / 100.0f;
                float DecreaseRate = LightBattery / NormalizeLightBlinkingPercentage;

                foreach (Light itLight in light)
                {
                    itLight.intensity =
                        Mathf.Sin(LightBlinkingInterval * (Mathf.PI / 180)) *
                        LightPower *
                        (NormalizeBlinkungDecrease * DecreaseRate + NormalizeBlinkungLowest);
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

        // UI反映（UVを切って表示、位置は動かさない）
        ApplyGaugeUI();

        //テキスト設定
        ApplyTextUI();

        // 状態保存
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.BatteryPieces = BatteryPieces;
            PlayerDataManager.Instance.BatteryAdditionPieces = BatteryAdditionPieces;
            PlayerDataManager.Instance.LightBattery = LightBattery;
        }
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
    /// バッテリー追加関数（これが「追加バッテリーをいじる入口」）
    /// </summary>
    /// <param name="AddPieces">追加するバッテリーの個数</param>
    public void AddBattery(int AddPieces)
    {
        BatteryAdditionPieces += AddPieces;

        // UI即時反映
        ApplyGaugeUI();
        ApplyTextUI();
    }

    public void ReduceBatteryByPercent(float percent)
    {
        damageAmount += percent / 100.0f;
    }

    /// <summary>
    /// ゲージImageを「Filled」で減少表現に設定する
    /// ※要求により fillOrigin は絶対に Bottom 固定
    /// </summary>
    /// <param name="img">対象のImage</param>
    private void SetupGaugeImage(Image img)
    {
        if (img == null)
            return;

        // RectTransformを触らない（位置が動く原因になるため）
        img.rectTransform.localScale = Vector3.one;

        // UVを切って減少：Filled + Vertical
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Vertical;

        // 絶対Bottom固定（Topは禁止）
        img.fillOrigin = (int)Image.OriginVertical.Bottom;

        // 縦ゲージでは影響しないが明示
        img.fillClockwise = true;
    }


    /// <summary>
    /// 追加→初期の順でバッテリーを消費し、LightBatteryの繰り越しを正しく処理する
    /// </summary>
    private void ConsumeBatteryWithCarry()
    {
        // LightBattery が 0 以下になった分（マイナス）を繰り越す
        float carry = LightBattery; // 例: -0.2 なら 0.2 分過剰消費した

        while (carry <= 0.0f)
        {
            // 追加バッテリーを優先消費
            if (BatteryAdditionPieces > 0)
            {
                BatteryAdditionPieces--;
            }
            else
            {
                --BatteryPieces;
            }

            // どちらも尽きたら消灯 → ゲームオーバー
            if (BatteryPieces <= 0 && BatteryAdditionPieces <= 0)
            {
                BatteryPieces = 0;
                BatteryAdditionPieces = 0;
                LightBattery = 0.0f;
                isLighting = false;

                //ライトを点灯させないための処理
                foreach (Light itLight in light)
                {
                    itLight.intensity = 0.0f;
                }
                
                // バッテリー切れ → 敵に捕まったのと同様にスコア0で日終了
                if (GameManager.Instance != null)
                {
                    Debug.Log("[PlayerLight] Battery depleted! Triggering day failure.");
                    GameManager.Instance.HandleDayFailed();
                }
                
                return;
            }

            // 1本分回復（1.0）して、繰り越し(マイナス)を足し戻す
            carry += 1.0f;
        }

        // 0～1にクランプ
        LightBattery = Mathf.Clamp01(carry);
    }

    /// <summary>
    /// ゲージUI反映（下=初期、上=追加。位置は入れ替えない）
    /// </summary>
    private void ApplyGaugeUI()
    {
        if (BatteryImage == null || AdditionBatteryImage == null)
            return;

        // 保険：毎フレーム Bottom を強制（外部からTopにされても勝つ）
        BatteryImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        AdditionBatteryImage.fillOrigin = (int)Image.OriginVertical.Bottom;

        bool usingAddition = (BatteryAdditionPieces > 0);

        if (usingAddition)
        {
            // 上（追加）が現在の残量、下（初期）は温存＝満タン表示
            AdditionBatteryImage.fillAmount = Mathf.Clamp01(LightBattery);
            BatteryImage.fillAmount = 1.0f;
        }
        else
        {
            // 上（追加）は空、下（初期）が現在の残量
            AdditionBatteryImage.fillAmount = 0.0f;
            BatteryImage.fillAmount = Mathf.Clamp01(LightBattery);
        }
    }


    /// <summary>
    /// テキストUI反映
    /// </summary>
    private void ApplyTextUI()
    {
        if (BatteryPiecesText != null)
        {
            BatteryPiecesText.SetText("{0}×", BatteryPieces);
        }
        if (AdditionPiecesText != null)
        {
            AdditionPiecesText.SetText("{0}×", BatteryAdditionPieces);
        }
    }

    /// <summary>
    /// 「追加が初期の位置に移動して見える」「下から上に消える」等の根本原因を、Start時ログで確定させる。
    /// </summary>
    private void ValidateGaugeSetup()
    {
        if (BatteryLife == null || AdditionBatteryLife == null)
        {
            Debug.LogError("PlayerLight: BatteryLife / AdditionBatteryLife が未設定です。", this);
            return;
        }

        if (BatteryLife == AdditionBatteryLife)
        {
            Debug.LogError("PlayerLight: BatteryLife と AdditionBatteryLife が同じGameObjectを参照しています（これだと『追加が初期の位置で減る』になります）。", this);
        }

        if (BatteryImage == null)
        {
            Debug.LogError($"PlayerLight: BatteryLife '{BatteryLife.name}' に Image がありません。『枠』や『親』を刺している可能性があります。", this);
        }
        if (AdditionBatteryImage == null)
        {
            Debug.LogError($"PlayerLight: AdditionBatteryLife '{AdditionBatteryLife.name}' に Image がありません。『枠』や『親』を刺している可能性があります。", this);
        }

        // 親にLayoutGroupがあると「見え方」によって詰まって移動して見える可能性がある
        var layoutA = BatteryLife.GetComponentInParent<LayoutGroup>();
        var layoutB = AdditionBatteryLife.GetComponentInParent<LayoutGroup>();
        if (layoutA != null || layoutB != null)
        {
            Debug.LogWarning("PlayerLight: ゲージの親階層に LayoutGroup が存在します。見え方/有効状態によってUIが詰まって『移動して見える』可能性があります。ゲージ部分はLayout管理から外すのが安全です。", this);
        }

        // 上下反転チェック（これがあると「下から上に消える」ように見える）
        float lossyY_A = BatteryLife.transform.lossyScale.y;
        float lossyY_B = AdditionBatteryLife.transform.lossyScale.y;
        if (lossyY_A < 0.0f || lossyY_B < 0.0f)
        {
            Debug.LogWarning("PlayerLight: ゲージ（または親）のYスケールが負です。上下反転しているため、FillOrigin=Topでも『下から上に消える』ように見えます。RectTransform/親CanvasのScaleを確認してください。", this);
        }

        Debug.Log($"PlayerLight GaugeRefs: BatteryLife='{BatteryLife.name}' (Image={(BatteryImage != null ? BatteryImage.name : "null")}) / AdditionBatteryLife='{AdditionBatteryLife.name}' (Image={(AdditionBatteryImage != null ? AdditionBatteryImage.name : "null")})", this);
    }
}
