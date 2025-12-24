using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultPanel : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI finalScoreText;
    [SerializeField] public Button backToSelectButton;

    private void OnEnable()
    {
        if (PlayerDataManager.Instance != null && finalScoreText != null)
        {
            finalScoreText.text = $"Final Score: {PlayerDataManager.Instance.CurrentScore}";
        }
    }

    private void Start()
    {
        if (backToSelectButton != null)
        {
            backToSelectButton.onClick.AddListener(OnBackToSelectClicked);
        }
    }

    private void OnBackToSelectClicked()
    {
        GameManager.Instance.ChangeState(GameState.Title);
    }

    // Auto-Link helper for UIBuilder
    [ContextMenu("Auto Link References")]
    public void AutoLink()
    {
        finalScoreText = transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        backToSelectButton = transform.Find("ReturnButton")?.GetComponent<Button>();
    }
}
