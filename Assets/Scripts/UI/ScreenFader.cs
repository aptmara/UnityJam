using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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


        if (fadeImage == null)
        {
            // Canvasがない場合は作成
            Canvas canvas = GetComponent<Canvas>();
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
        }

        // 初期状態は黒（不透明）にしておく
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
        fadeImage.gameObject.SetActive(true);
    }

    public void FadeIn(float duration = -1f)
    {
        StartCoroutine(FadeRoutine(1f, 0f, duration < 0 ? defaultFadeDuration : duration));
    }

    public void FadeOut(float duration = -1f)
    {
        StartCoroutine(FadeRoutine(0f, 1f, duration < 0 ? defaultFadeDuration : duration));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        fadeImage.gameObject.SetActive(true);
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
    }
}
