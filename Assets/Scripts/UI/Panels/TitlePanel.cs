using UnityEngine;
using UnityEngine.UI;

public class TitlePanel : MonoBehaviour
{
    [SerializeField] public Button startButton;

    private void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        GameManager.Instance.ChangeState(GameState.Select);
    }

    [ContextMenu("Auto Link References")]
    public void AutoLink()
    {
        startButton = transform.Find("StartButton")?.GetComponent<Button>();
    }
}
