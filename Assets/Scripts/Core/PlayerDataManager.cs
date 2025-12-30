using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }

    public int CurrentScore { get; private set; }
    public float BatteryLife { get; private set; }
    public int Gold { get; private set; }

    // 進行状況管理
    public int CurrentStageIndex { get; set; } = 0;
    public int CurrentPlayerIndex { get; set; } = 0;

    // バッテリー永続化 (初期値はPlayerLightのInspector値に依存するが、ここで保持)
    // 初期化は初回Start時にPlayerLightから書き込むか、ここで定義するか。
    // PlayerLightのInspector値を優先するため、最初は未設定(-1など)にしておく手もあるが、
    // ここでは単純に保持用変数として定義。
    public int BatteryPieces { get; set; } = -1; // -1 indicates not initialized
    public int BatteryAdditionPieces { get; set; } = 0;
    public float LightBattery { get; set; } = 1.0f;

    public void AdvanceStage()
    {
        CurrentStageIndex++;
    }

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

    public void AddScore(int amount)
    {
        CurrentScore += amount;
    }

    public void ResetScore()
    {
        CurrentScore = 0;
    }

    public void SetBattery(float amount)
    {
        BatteryLife = amount;
    }

    public void DeductBattery(float amount)
    {
        BatteryLife = Mathf.Max(0, BatteryLife - amount);
    }

    /// <summary>
    /// データをリセットする（タイトルに戻る時など）
    /// </summary>
    public void ResetData()
    {
        ResetScore();
        // Inspectorで設定されている初期値に戻すため、-1にする
        BatteryPieces = -1;
        BatteryAdditionPieces = 0;
        LightBattery = 1.0f;

        // 進行度もリセット
        CurrentStageIndex = 0;
    }
}
