using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] public Button retryButton;
    [SerializeField] public Button returnButton;

    private void Start()
    {
        if(retryButton) retryButton.onClick.AddListener(OnRetryClicked);
        if(returnButton) returnButton.onClick.AddListener(OnReturnClicked);
    }

    private void OnRetryClicked()
    {
        // 簡易リトライ: 同じステージへ等
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOut(-1f, () => 
            {
                GameManager.Instance.ChangeState(GameState.Gameplay);
            });
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.Gameplay);
        }
    }

    private void OnReturnClicked()
    {
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOut(-1f, () => 
            {
                GameManager.Instance.ChangeState(GameState.Title);
            });
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.Title);
        }
    }

    [ContextMenu("Auto Link References")]
    public void AutoLink()
    {
        retryButton = transform.Find("RetryButton")?.GetComponent<Button>();
        returnButton = transform.Find("ReturnButton")?.GetComponent<Button>();
    }
}
