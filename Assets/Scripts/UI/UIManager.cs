using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("State UI Prefabs")]
    [SerializeField] private List<GameObject> titlePrefabs;
    [SerializeField] private List<GameObject> selectPrefabs;
    [SerializeField] private List<GameObject> stageIntroPrefabs;
    [SerializeField] private List<GameObject> gameplayPrefabs;
    [SerializeField] private List<GameObject> resultPrefabs;
    [SerializeField] private List<GameObject> gameOverPrefabs;

    private List<GameObject> currentUIInstances = new List<GameObject>();

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

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged += HandleStateChanged;
            // 初期状態反映
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        // 既存のUIを全て削除
        foreach (var instance in currentUIInstances)
        {
            if (instance != null) Destroy(instance);
        }
        currentUIInstances.Clear();

        List<GameObject> prefabsToInstantiate = null;

        switch (state)
        {
            case GameState.Title:
                prefabsToInstantiate = titlePrefabs;
                break;
            case GameState.Select:
                prefabsToInstantiate = selectPrefabs;
                break;
            case GameState.StageIntro:
                prefabsToInstantiate = stageIntroPrefabs;
                break;
            case GameState.Gameplay:
                prefabsToInstantiate = gameplayPrefabs;
                break;
            case GameState.Result:
                prefabsToInstantiate = resultPrefabs;
                break;
            case GameState.GameOver:
                prefabsToInstantiate = gameOverPrefabs;
                break;
            // 必要に応じて他のステートも追加
        }

        if (prefabsToInstantiate != null)
        {
            foreach (var prefab in prefabsToInstantiate)
            {
                if (prefab != null)
                {
                    GameObject uiObj = Instantiate(prefab, transform);
                    currentUIInstances.Add(uiObj);
                }
            }
        }
    }

    [Header("UI Resources")]
    [SerializeField] public TMP_FontAsset uiFont;

#if UNITY_EDITOR
    // --- Cave Exploration Theme Generators (PvE / Underground) ---

    // Colors: Dark Stone, Bioluminescent Teal, Danger Orange
    private static Color ColorCaveBg = new Color(0.12f, 0.12f, 0.14f, 0.95f); 
    private static Color ColorCaveAccent = new Color(0f, 0.85f, 0.9f, 1f); 
    private static Color ColorCaveWarning = new Color(1f, 0.3f, 0f, 1f); 
    private static Color ColorCaveText = new Color(0.9f, 0.9f, 0.95f);
    
    [ContextMenu("Generate All UIs (Cave Theme)")]
    public void GenerateAll_Cave()
    {
        // cleanup
        DestroyExistingRoot("TitleUI_Cave");
        DestroyExistingRoot("SelectUI_Cave");
        DestroyExistingRoot("ResultUI_Cave");
        DestroyExistingRoot("GameOverUI_Cave");

        CreateTitleUI_Cave();
        CreateSelectUI_Cave();
        CreateResultUI_Cave();
        CreateGameOverUI_Cave();
    }

    [ContextMenu("Generate Title UI (Cave)")]
    public void CreateTitleUI_Cave()
    {
        GameObject root = CreatePanelRoot("TitleUI_Cave");
        root.AddComponent<TitlePanel>();

        CreateImage(root, "Background", ColorCaveBg, true);
        // Vignette for atmosphere
        GameObject vig = CreateImage(root, "Vignette", Color.black, true);
        vig.GetComponent<Image>().color = new Color(0,0,0,0.5f);

        // Main Content (Center)
        GameObject content = CreateVerticalGroup(root, "Content", 40, TextAnchor.MiddleCenter);
        
        // Title with Gradient
        var title = CreateText(content, "TitleMain", "DOME CANNON", 100, Color.white);
        title.lineSpacing = -10;
        title.enableVertexGradient = true;
        title.colorGradient = new VertexGradient(Color.white, Color.white, new Color(0.6f, 0.7f, 0.8f), new Color(0.3f, 0.35f, 0.4f));
        title.fontStyle = FontStyles.Bold; // Bold title is fine for logo-feel

        CreateSpacer(content, 60);

        // Buttons
        CreateCaveButton(content, "StartButton", "出撃", ColorCaveAccent);

        // Decor
        CreateCaveDecor(root);

        Debug.Log("Cave Title UI Generated.");
        Undo.RegisterCreatedObjectUndo(root, "Create Title UI Cave");
    }

    [ContextMenu("Generate Select UI (Cave)")]
    public void CreateSelectUI_Cave()
    {
        GameObject root = CreatePanelRoot("SelectUI_Cave");
        root.AddComponent<SelectPanel>();

        CreateImage(root, "Background", ColorCaveBg, true);
        GameObject vig = CreateImage(root, "Vignette", Color.black, true);
        vig.GetComponent<Image>().color = new Color(0,0,0,0.5f);

        // Header
        GameObject topBar = CreateContainer(root, "TopBar", TextAnchor.UpperCenter, new Vector2(0, 0.85f), new Vector2(1, 1));
        CreateImage(topBar, "BarBg", new Color(0,0,0,0.6f), true);
        var head = CreateText(topBar, "Header", "ステージ選択", 36, ColorCaveText);
        head.rectTransform.anchoredPosition = new Vector2(0, -35);
        head.enableVertexGradient = true;
        head.colorGradient = new VertexGradient(Color.white, Color.white, Color.gray, Color.gray);

        // Center Content
        GameObject content = CreateVerticalGroup(root, "Menu", 25, TextAnchor.MiddleCenter);
        
        CreateCaveButton(content, "DungeonButton", "鉱山", ColorCaveAccent);
        CreateCaveButton(content, "ShopButton", "ショップ");
        CreateCaveButton(content, "OptionsButton", "設定");

        CreateCaveDecor(root);

        Debug.Log("Cave Select UI Generated.");
        Undo.RegisterCreatedObjectUndo(root, "Create Select UI Cave");
    }

    [ContextMenu("Generate Result UI (Cave)")]
    public void CreateResultUI_Cave()
    {
        GameObject root = CreatePanelRoot("ResultUI_Cave");
        root.AddComponent<ResultPanel>();

        CreateImage(root, "Overlay", new Color(0, 0, 0, 0.85f), true);

        // Panel
        GameObject panel = CreateVerticalGroup(root, "ResultPanel", 20, TextAnchor.MiddleCenter);
        
        var title = CreateText(panel, "Title", "リザルト", 48, ColorCaveAccent);
        title.enableVertexGradient = true;
        title.colorGradient = new VertexGradient(ColorCaveAccent, ColorCaveAccent, Color.blue, Color.blue);
        
        CreateSeparator(panel, Color.gray);

        CreateText(panel, "ScoreLabel", "スコア", 24, Color.gray);
        CreateText(panel, "ScoreText", "12450", 72, Color.white);
        
        CreateSpacer(panel, 60);

        CreateCaveButton(panel, "ReturnButton", "タイトルへ");

        Debug.Log("Cave Result UI Generated.");
        Undo.RegisterCreatedObjectUndo(root, "Create Result UI Cave");
    }

    [ContextMenu("Generate Game Over UI (Cave)")]
    public void CreateGameOverUI_Cave()
    {
        GameObject root = CreatePanelRoot("GameOverUI_Cave");
        root.AddComponent<GameOverPanel>();

        CreateImage(root, "Bg", new Color(0.2f, 0.05f, 0.05f, 0.95f), true); // Reddish dark
        GameObject vig = CreateImage(root, "Vignette", Color.black, true);
        vig.GetComponent<Image>().color = new Color(0,0,0,0.6f);
        
        GameObject content = CreateVerticalGroup(root, "Content", 40, TextAnchor.MiddleCenter);
        
        var title = CreateText(content, "FailTitle", "ゲームオーバー", 80, ColorCaveWarning);
        title.enableVertexGradient = true;
        title.colorGradient = new VertexGradient(Color.red, Color.red, Color.black, Color.black);
        
        CreateSpacer(content, 60);
        
        CreateCaveButton(content, "RetryButton", "リトライ", ColorCaveWarning);
        CreateCaveButton(content, "ReturnButton", "タイトルへ");

        Debug.Log("Cave Game Over UI Generated.");
        Undo.RegisterCreatedObjectUndo(root, "Create Game Over UI Cave");
    }


    // --- Helpers ---

    private void DestroyExistingRoot(string name)
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform t = canvas.transform.Find(name);
            if (t != null)
            {
                DestroyImmediate(t.gameObject);
            }
        }
    }

    private GameObject CreateCaveButton(GameObject parent, string name, string label, Color? accent = null)
    {
        Color ac = accent ?? Color.white;
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent.transform, false);
        
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 70);
        
        // Rough stone-like Bg
        Image img = btnObj.GetComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.28f, 1f); // Lighter stone
        
        Button btn = btnObj.GetComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.4f, 0.4f, 0.45f);
        cb.pressedColor = new Color(0.6f, 0.6f, 0.65f);
        btn.colors = cb;
        
        // Outline / Shadow
        GameObject shadow = CreateImage(btnObj, "Shadow", new Color(0,0,0,0.5f));
        shadow.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        shadow.GetComponent<RectTransform>().anchorMax = Vector2.one;
        shadow.GetComponent<RectTransform>().offsetMin = new Vector2(4, -4);
        shadow.GetComponent<RectTransform>().offsetMax = new Vector2(4, -4);
        shadow.transform.SetAsFirstSibling();

        // Accent Bar
        GameObject bar = CreateImage(btnObj, "Bar", ac);
        RectTransform brt = bar.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0,0); brt.anchorMax = new Vector2(0,1);
        brt.sizeDelta = new Vector2(6, 0);
        brt.anchoredPosition = new Vector2(3, 0);

        // Text
        var txt = CreateText(btnObj, "Label", label, 28, Color.white);
        txt.rectTransform.sizeDelta = new Vector2(380, 70);
        txt.alignment = TextAlignmentOptions.Center; 
        
        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.minHeight = 70; le.minWidth = 400;

        return btnObj;
    }

    private void CreateCaveDecor(GameObject parent)
    {
        GameObject frame = CreateContainer(parent, "DecorFrame", TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        Stretch(frame.GetComponent<RectTransform>());
        
        float margin = 20;
        // Thicker, more industrial/cave brackets
        var tl = CreateImage(frame, "TL", ColorCaveAccent);
        tl.GetComponent<RectTransform>().anchorMin = new Vector2(0,1);
        tl.GetComponent<RectTransform>().anchorMax = new Vector2(0,1);
        tl.GetComponent<RectTransform>().anchoredPosition = new Vector2(margin, -margin);
        tl.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 4);

        var tl2 = CreateImage(frame, "TL2", ColorCaveAccent);
        tl2.GetComponent<RectTransform>().anchorMin = new Vector2(0,1);
        tl2.GetComponent<RectTransform>().anchorMax = new Vector2(0,1);
        tl2.GetComponent<RectTransform>().anchoredPosition = new Vector2(margin, -margin);
        tl2.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 80);

        var br = CreateImage(frame, "BR", ColorCaveAccent);
        br.GetComponent<RectTransform>().anchorMin = new Vector2(1,0);
        br.GetComponent<RectTransform>().anchorMax = new Vector2(1,0);
        br.GetComponent<RectTransform>().anchoredPosition = new Vector2(-margin, margin);
        br.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 4);
        
        var br2 = CreateImage(frame, "BR2", ColorCaveAccent);
        br2.GetComponent<RectTransform>().anchorMin = new Vector2(1,0);
        br2.GetComponent<RectTransform>().anchorMax = new Vector2(1,0);
        br2.GetComponent<RectTransform>().anchoredPosition = new Vector2(-margin, margin);
        br2.GetComponent<RectTransform>().sizeDelta = new Vector2(4, 80);
    }

    private GameObject CreatePanelRoot(string name)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas != null) root.transform.SetParent(canvas.transform, false);
        Stretch(root.GetComponent<RectTransform>());
        return root;
    }

    private GameObject CreateVerticalGroup(GameObject parent, string name, float spacing, TextAnchor align)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);
        Stretch(obj.GetComponent<RectTransform>()); 
        
        VerticalLayoutGroup vlg = obj.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = align;
        vlg.spacing = spacing;
        
        return obj;
    }
    
    private GameObject CreateContainer(GameObject parent, string name, TextAnchor anchor, Vector2 pivot, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);
        Stretch(obj.GetComponent<RectTransform>()); 
        return obj;
    }

    private GameObject CreateImage(GameObject parent, string name, Color color, bool fillStretch = false)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent.transform, false);
        obj.GetComponent<Image>().color = color;
        if (fillStretch) Stretch(obj.GetComponent<RectTransform>());
        return obj;
    }

    private TextMeshProUGUI CreateText(GameObject parent, string name, string content, float fontSize, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent.transform, false);
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.color = color;
        if (uiFont != null) tmp.font = uiFont;
        tmp.alignment = TextAlignmentOptions.Center;
        
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.minHeight = fontSize * 1.2f;
        le.preferredHeight = fontSize * 1.5f;
        
        return tmp;
    }
    
    private void CreateSpacer(GameObject parent, float height)
    {
        GameObject obj = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        obj.transform.SetParent(parent.transform, false);
        obj.GetComponent<LayoutElement>().preferredHeight = height;
    }
    
    private void CreateSeparator(GameObject parent, Color color)
    {
        GameObject sep = CreateImage(parent, "Separator", color);
        LayoutElement le = sep.AddComponent<LayoutElement>();
        le.minHeight = 2;
        le.preferredHeight = 2;
        le.preferredWidth = 200;
    }

    private void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
#endif
}
