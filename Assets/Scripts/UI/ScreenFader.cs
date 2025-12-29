using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;

public class ScreenFader : MonoBehaviour
{
    private static ScreenFader _instance;
    public static ScreenFader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ScreenFader>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("ScreenFader");
                    _instance = obj.AddComponent<ScreenFader>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private Image fadeImage;
    [SerializeField] private float defaultFadeDuration = 1.0f;
    [SerializeField] private float defaultBlackHoldDuration = 1.0f;
    [SerializeField] private TMP_Text dayText; // Inspectorでアサイン

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Canvas canvas = null;

        if (fadeImage == null)
        {
            // Canvasがない場合は作成
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("FadeCanvas");
                canvasObj.transform.SetParent(transform);
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // 最前面
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Imageを作成
            GameObject imageObj = new GameObject("BlackOverlay");
            imageObj.transform.SetParent(canvas.transform, false);
            fadeImage = imageObj.AddComponent<Image>();
            fadeImage.color = Color.black;

            // 全画面ストレッチ
            RectTransform rt = fadeImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Debug.Log($"ScreenFader initialized with auto-generated Canvas. Image: {fadeImage.name}");
        }
        else
        {
            canvas = fadeImage.GetComponentInParent<Canvas>();
        }

        // dayTextがInspectorで未設定の場合は非表示にしておく
        if (dayText != null)
        {
            dayText.gameObject.SetActive(false);
        }

        // 初期状態は黒（不透明）にしておく
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(true);
    }

    public void FadeIn(float duration = -1f, float holdDuration = -1f)
    {
        Debug.Log("ScreenFader: FadeIn started");
        float fDur = duration < 0 ? defaultFadeDuration : duration;
        float hDur = holdDuration < 0 ? defaultBlackHoldDuration : holdDuration;
        StartCoroutine(FadeRoutine(1f, 0f, fDur, hDur, null));
    }

    /// <summary>
    /// 日数表示付きフェードイン（プレハブ版）
    /// </summary>
    public void FadeInWithDayText(int dayNumber, GameObject dayDisplayPrefab = null, float duration = -1f, float holdDuration = 2f)
    {
        Debug.Log($"ScreenFader: FadeIn with Day {dayNumber}");
        float fDur = duration < 0 ? defaultFadeDuration : duration;
        
        GameObject instantiatedUI = null;
        TMP_Text textToUse = null;
        
        // プレハブからインスタンス化
        if (dayDisplayPrefab != null)
        {
            // ScreenFaderのCanvasに配置
            Canvas canvas = fadeImage != null ? fadeImage.GetComponentInParent<Canvas>() : null;
            if (canvas != null)
            {
                instantiatedUI = Instantiate(dayDisplayPrefab, canvas.transform);
                textToUse = instantiatedUI.GetComponentInChildren<TMP_Text>();
            }
        }
        
        // フォールバック: SerializeFieldのdayTextを使用
        if (textToUse == null && dayText != null)
        {
            textToUse = dayText;
        }
        
        // 日数テキスト設定
        if (textToUse != null)
        {
            textToUse.text = $"{dayNumber}日目";
            textToUse.gameObject.SetActive(true);
        }
        
        StartCoroutine(FadeRoutineWithDayText(1f, 0f, fDur, holdDuration, null, textToUse, instantiatedUI));
    }

    public void FadeOut(float duration = -1f, Action onComplete = null)
    {
        Debug.Log("ScreenFader: FadeOut started");
        StartCoroutine(FadeRoutine(0f, 1f, duration < 0 ? defaultFadeDuration : duration, 0f, onComplete));
    }

    private IEnumerator FadeRoutineWithDayText(float startAlpha, float endAlpha, float duration, float holdDuration, Action onComplete, TMP_Text textToUse, GameObject instantiatedUI = null)
    {
        if (fadeImage == null)
        {
            if (instantiatedUI != null) Destroy(instantiatedUI);
            onComplete?.Invoke();
            yield break;
        }

        fadeImage.gameObject.SetActive(true);
        
        // テキスト表示中のホールド
        if (holdDuration > 0f)
        {
            Color cInitial = fadeImage.color;
            cInitial.a = startAlpha;
            fadeImage.color = cInitial;
            yield return new WaitForSeconds(holdDuration);
        }
        
        // インスタンス化されたUIを破棄（プレハブの場合）
        if (instantiatedUI != null)
        {
            Destroy(instantiatedUI);
        }
        else if (textToUse != null)
        {
            // SerializeFieldの場合は非表示にするだけ
            textToUse.gameObject.SetActive(false);
        }

        // フェード
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;

            yield return null;
        }

        Color finalColor = fadeImage.color;
        finalColor.a = endAlpha;
        fadeImage.color = finalColor;

        if (endAlpha <= 0f)
        {
            fadeImage.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration, float holdDuration, Action onComplete = null)
    {
        if (fadeImage == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        fadeImage.gameObject.SetActive(true);

        // Hold (黒画面維持など)
        if (holdDuration > 0f)
        {
            // 開始アルファを設定して待機
            Color cInitial = fadeImage.color;
            cInitial.a = startAlpha;
            fadeImage.color = cInitial;
            yield return new WaitForSeconds(holdDuration);
        }

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;

            yield return null;
        }

        Color finalColor = fadeImage.color;
        finalColor.a = endAlpha;
        fadeImage.color = finalColor;

        if (endAlpha <= 0f)
        {
            fadeImage.gameObject.SetActive(false);
        }

        onComplete?.Invoke();
    }
}
