using UnityEngine;
using TMPro;

public class InGamePanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI batteryText;

    private void Update()
    {
        if (PlayerDataManager.Instance != null)
        {

        }
    }
}
