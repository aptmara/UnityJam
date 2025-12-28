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

    [SerializeField] private List<GameObject> gameplayPrefabs;
    [SerializeField] private List<GameObject> resultPrefabs; // Used for Daily Result
    [SerializeField] private List<GameObject> shopPrefabs;
    [SerializeField] private List<GameObject> finalResultPrefabs;
    [SerializeField] private List<GameObject> gameOverPrefabs;
    [SerializeField] private List<GameObject> creditPrefabs; // Added

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
            case GameState.Gameplay:
                prefabsToInstantiate = gameplayPrefabs;
                break;
            case GameState.Result:
                prefabsToInstantiate = resultPrefabs;
                break;
            case GameState.Shop:
                prefabsToInstantiate = shopPrefabs;
                break;
            case GameState.FinalResult:
                prefabsToInstantiate = finalResultPrefabs;
                break;
            case GameState.Credits: // Added
                prefabsToInstantiate = creditPrefabs;
                break;
            case GameState.GameOver:
                prefabsToInstantiate = gameOverPrefabs;
                // 強制的にカーソル解放
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                break;
        }

        if (prefabsToInstantiate != null)
        {
            string canvasName = "SystemUICanvas";
            GameObject canvasObj = GameObject.Find(canvasName);
            Canvas canvas = null;

            if (canvasObj != null)
            {
                canvas = canvasObj.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                GameObject cObj = new GameObject(canvasName);
                canvas = cObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Ensure on top
                var scaler = cObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                cObj.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(cObj);
            }

            Transform parentTransform = canvas.transform;

                if (state == GameState.GameOver && (prefabsToInstantiate == null || prefabsToInstantiate.Count == 0))
                {
                    // プレハブがない場合、動的にUI生成
                    CreateDefaultGameOverUI(parentTransform);
                }
                else if (prefabsToInstantiate != null)
                {
                    foreach (var prefab in prefabsToInstantiate)
                    {
                        if (prefab != null)
                        {
                            // falseを指定してプレハブのローカル設定（位置・スケール）を維持
                            GameObject uiObj = Instantiate(prefab, parentTransform, false);
                            currentUIInstances.Add(uiObj);

                            // Auto-bind click sound to all buttons
                            var buttons = uiObj.GetComponentsInChildren<Button>(true);
                            foreach (var btn in buttons)
                            {
                                btn.onClick.AddListener(() => 
                                {
                                    if (UnityJam.Core.SoundManager.Instance != null)
                                    {
                                        UnityJam.Core.SoundManager.Instance.PlayUIClick();
                                    }
                                });
                            }
                        }
                    }
                }
        }
    }

    private void CreateDefaultGameOverUI(Transform parent)
    {
        GameObject goObj = new GameObject("GameOverUI_Auto");
        goObj.transform.SetParent(parent, false);
        RectTransform rt = goObj.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // スクリプト追加
        var logic = goObj.AddComponent<UnityJam.UI.GameOverUI>();
        currentUIInstances.Add(goObj);

        // テキスト生成
        GameObject txtObj = new GameObject("Text_GameOver");
        txtObj.transform.SetParent(goObj.transform, false);
        var tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "GAME OVER";
        tmp.fontSize = 80;
        tmp.color = Color.red;
        tmp.alignment = TextAlignmentOptions.Center;
        RectTransform tmpRt = txtObj.GetComponent<RectTransform>();
        tmpRt.anchorMin = new Vector2(0.5f, 0.6f);
        tmpRt.anchorMax = new Vector2(0.5f, 0.6f);
        tmpRt.pivot = new Vector2(0.5f, 0.5f);
        tmpRt.sizeDelta = new Vector2(600, 150);
        tmpRt.anchoredPosition = Vector2.zero;

        // ボタン生成
        GameObject btnObj = new GameObject("Button_Return");
        btnObj.transform.SetParent(goObj.transform, false);
        var img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var btn = btnObj.AddComponent<Button>();
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0.3f);
        btnRt.anchorMax = new Vector2(0.5f, 0.3f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.sizeDelta = new Vector2(240, 60);
        btnRt.anchoredPosition = Vector2.zero;

        // ボタンテキスト
        GameObject btnTxtObj = new GameObject("Text");
        btnTxtObj.transform.SetParent(btnObj.transform, false);
        var btnTmp = btnTxtObj.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "RETURN TO TITLE";
        btnTmp.fontSize = 24;
        btnTmp.color = Color.white;
        btnTmp.alignment = TextAlignmentOptions.Center;
        RectTransform btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
        btnTxtRt.anchorMin = Vector2.zero;
        btnTxtRt.anchorMax = Vector2.one;
        btnTxtRt.offsetMin = Vector2.zero;
        btnTxtRt.offsetMax = Vector2.zero;

        // リフレクションでprivateフィールドに参照をセット
        System.Reflection.FieldInfo btnField = typeof(UnityJam.UI.GameOverUI).GetField("returnButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (btnField != null) btnField.SetValue(logic, btn);

        System.Reflection.FieldInfo txtField = typeof(UnityJam.UI.GameOverUI).GetField("gameOverText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (txtField != null) txtField.SetValue(logic, tmp);
    }
}
