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
}
