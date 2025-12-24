using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : MonoBehaviour
{
    [SerializeField] public Button startButton;
    [SerializeField] public Button creditsButton;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);
    }

    private void OnStartClicked()
    {
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

    private void OnCreditsClicked()
    {
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOut(-1f, () => 
            {
                GameManager.Instance.ChangeState(GameState.Credits);
            });
        }
        else
        {
            GameManager.Instance.ChangeState(GameState.Credits);
        }
    }

    [ContextMenu("Auto Link References")]
    public void AutoLink()
    {
        startButton = transform.Find("StartButton")?.GetComponent<Button>();
        creditsButton = transform.Find("CreditsButton")?.GetComponent<Button>();
    }
}
