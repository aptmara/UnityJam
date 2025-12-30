using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : MonoBehaviour
{
    [SerializeField] public Button startButton;
    [SerializeField] public Button creditsButton;
    [SerializeField] private Transform volumeSliderContainer; // Container for the slider

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnCreditsClicked);

        // Instantiate Volume Slider
        if (UnityJam.Core.SoundManager.Instance != null && 
            UnityJam.Core.SoundManager.Instance.volumeSliderPrefab != null)
        {
            if (volumeSliderContainer != null)
            {
                Instantiate(UnityJam.Core.SoundManager.Instance.volumeSliderPrefab, volumeSliderContainer);
            }
            else
            {
                // Fallback: Instantiate as child of this panel if no container specified
                Instantiate(UnityJam.Core.SoundManager.Instance.volumeSliderPrefab, transform);
            }
        }
    }

    private void OnStartClicked()
    {
        // ゲーム開始前にセッション状態をリセット
        if (UnityJam.Core.GameSessionManager.Instance != null)
        {
            UnityJam.Core.GameSessionManager.Instance.ResetSession();
        }
        
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
