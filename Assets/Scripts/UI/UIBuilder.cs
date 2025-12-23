#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

public class UIBuilder : MonoBehaviour
{
    // コンテキストメニューから実行可能にする
    [ContextMenu("Generate All Default UIs")]
    public void GenerateAll()
    {
        CreateTitleUI();
        CreateSelectUI();
        CreateResultUI();
        CreateGameOverUI();
    }

    [ContextMenu("Generate Title UI")]
    public void CreateTitleUI()
    {
        GameObject root = CreatePanelRoot("TitleUI_Generated");
        TitlePanel panelScript = root.AddComponent<TitlePanel>();

        // Background
        CreateImage(root, "Background", Color.black, true);

        // Title Text
        var titleText = CreateText(root, "TitleText", "DOME CANNON", 64, new Vector2(0, 100));
        
        // Start Button
        var startBtnObj = CreateButton(root, "StartButton", "START GAME", new Vector2(0, -50));
        
        // UIスクリプトへの参照セットアップ (Reflection or SerializedObject)
        // ここでは簡易的に名前等で合わせる前提、またはユーザーがアサインする
        // TitlePanelスクリプト側が public Button startButton; を持っていれば自動検索するロジックも書けるが
        // 今回は生成のみ行い、アサインはInspectorで行ってもらう形が安全。
        
        Debug.Log("Title UI Generated. Please assign button references in Inspector.");
        Undo.RegisterCreatedObjectUndo(root, "Create Title UI");
    }

    [ContextMenu("Generate Select UI")]
    public void CreateSelectUI()
    {
        GameObject root = CreatePanelRoot("SelectUI_Generated");
        SelectPanel panelScript = root.AddComponent<SelectPanel>();

        CreateImage(root, "Background", new Color(0.1f, 0.1f, 0.2f, 1f), true);
        CreateText(root, "Header", "SELECT MODE", 48, new Vector2(0, 300));

        CreateButton(root, "DungeonButton", "GO TO DUNGEON", new Vector2(0, 50));
        CreateButton(root, "ShopButton", "SHOP", new Vector2(0, -50));

        Debug.Log("Select UI Generated.");
        Undo.RegisterCreatedObjectUndo(root, "Create Select UI");
    }

    [ContextMenu("Generate Result UI")]
    public void CreateResultUI()
    {
        GameObject root = CreatePanelRoot("ResultUI_Generated");
        ResultPanel panelScript = root.AddComponent<ResultPanel>();

        CreateImage(root, "Background", new Color(0, 0, 0, 0.8f), true);
        CreateText(root, "Header", "RESULT", 56, new Vector2(0, 200));
        CreateText(root, "ScoreText", "Score: 0", 36, Vector2.zero);

        CreateButton(root, "ReturnButton", "RETURN TO BASE", new Vector2(0, -200));

        Debug.Log("Result UI Generated.");
        Undo.RegisterCreatedObjectUndo(root, "Create Result UI");
    }

    [ContextMenu("Generate Game Over UI")]
    public void CreateGameOverUI()
    {
        GameObject root = CreatePanelRoot("GameOverUI_Generated");
        GameOverPanel panelScript = root.AddComponent<GameOverPanel>();

        CreateImage(root, "Background", new Color(0.5f, 0, 0, 0.8f), true);
        CreateText(root, "Header", "MISSION FAILED", 64, new Vector2(0, 100), Color.red);

        CreateButton(root, "RetryButton", "RETRY", new Vector2(0, -50));
        CreateButton(root, "ReturnButton", "RETURN", new Vector2(0, -150));

        Debug.Log("Game Over UI Generated.");
        Undo.RegisterCreatedObjectUndo(root, "Create Game Over UI");
    }

    // --- Helpers ---

    private GameObject CreatePanelRoot(string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        // 親をCanvasにする検索処理
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            root.transform.SetParent(canvas.transform, false);
        }
        
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        return root;
    }

    private GameObject CreateImage(GameObject parent, string name, Color color, bool fillStretch = false)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent.transform, false);
        obj.GetComponent<Image>().color = color;
        
        if (fillStretch)
        {
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        return obj;
    }

    private TextMeshProUGUI CreateText(GameObject parent, string name, string content, float fontSize, Vector2 anchoredPos, Color? color = null)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color ?? Color.white;
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, 100);
        rt.anchoredPosition = anchoredPos;
        
        return tmp;
    }

    private GameObject CreateButton(GameObject parent, string name, string labelText, Vector2 anchoredPos)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent.transform, false);
        
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(240, 60);
        rt.anchoredPosition = anchoredPos;
        
        btnObj.GetComponent<Image>().color = Color.white;
        
        // Text Child
        CreateText(btnObj, "Text", labelText, 24, Vector2.zero, Color.black);
        
        return btnObj;
    }
}
#endif
