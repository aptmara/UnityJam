using UnityEngine;
using UnityEngine.UI;

public class SelectPanel : MonoBehaviour
{
    [SerializeField] public Button startDungeonButton;
    [SerializeField] public Button shopButton;

    private void Start()
    {
        if(startDungeonButton) startDungeonButton.onClick.AddListener(OnStartDungeonClicked);
        if(shopButton) shopButton.onClick.AddListener(OnShopClicked);
    }

    private void OnStartDungeonClicked()
    {
        GameManager.Instance.ChangeState(GameState.Gameplay);
    }

    private void OnShopClicked()
    {
        Debug.Log("Open Shop UI");
    }

    [ContextMenu("Auto Link References")]
    public void AutoLink()
    {
        startButtonDungeonButton = transform.Find("DungeonButton")?.GetComponent<Button>();
        shopButton = transform.Find("ShopButton")?.GetComponent<Button>();
    }
    
    // Typo fix helper
    private Button startButtonDungeonButton { set { startDungeonButton = value; } }
}
