using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace UnityJam.Credits
{
    public class CreditGameManager : MonoBehaviour
    {
        public static CreditGameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private CyberRailCamera railCamera;
        [SerializeField] private PlayerShip playerShip;
        [SerializeField] private Transform spawnOrigin;
        [SerializeField] private GameObject creditPrefab;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private UnityEngine.UI.Slider hpSlider;

        [SerializeField] private UnityEngine.UI.Slider bossHpSlider;
        [SerializeField] private GameObject creditNamePrefab;


        [System.Serializable]
        public class CreditSection
        {
            public string Role;
            public List<string> Names = new List<string>();
        }

        private List<CreditSection> creditSections = new List<CreditSection>();


        private int totalScore = 0;
        private bool isPhaseActive = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {

            if (railCamera == null)
            {
                railCamera = FindFirstObjectByType<CyberRailCamera>();
                if (railCamera == null)
                {
                    GameObject camObj = new GameObject("CyberRailCamera");
                    camObj.tag = "MainCamera";
                    var cam = camObj.AddComponent<Camera>();
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = Color.black;
                    cam.orthographic = true;
                    cam.orthographicSize = 5f;
                    camObj.transform.position = new Vector3(0, 0, -10f);
                    camObj.AddComponent<AudioListener>();
                    railCamera = camObj.AddComponent<CyberRailCamera>();
                }
            }


            if (railCamera != null)
            {
                var cam = railCamera.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = Color.black;
                    cam.orthographic = true;
                    cam.orthographicSize = 5f;
                    cam.transform.position = new Vector3(0, 0, -10f);

#if UNITY_EDITOR || !UNITY_SERVER
                    var urpData = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                    if (urpData == null)
                    {
                        urpData = cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                    }
                    urpData.renderType = UnityEngine.Rendering.Universal.CameraRenderType.Base;
                    urpData.renderPostProcessing = true;
#endif
                }
            }


            if (FindFirstObjectByType<Starfield>() == null)
            {
                new GameObject("Starfield").AddComponent<Starfield>();
            }

            SetupUI();
            LoadCredits();

            if (playerShip != null)
            {
                Camera cam = railCamera != null ? railCamera.GetComponent<Camera>() : null;
                playerShip.Initialize(cam);
            }

            if (ScreenFader.Instance != null) ScreenFader.Instance.FadeIn();

            StartCoroutine(MainGameLoop());
        }

        private void LoadCredits()
        {
            TextAsset txt = Resources.Load<TextAsset>("Credits");
            if (txt == null) return;

            string[] lines = txt.text.Split('\n');
            CreditSection currentSection = null;

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("["))
                {
                    currentSection = new CreditSection();
                    currentSection.Role = line.Replace("[", "").Replace("]", "");
                    creditSections.Add(currentSection);
                }
                else
                {
                    if (currentSection != null)
                    {
                        currentSection.Names.Add(line);
                    }
                }
            }
        }

        public void AddScore(int val)
        {
            totalScore += val;
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {totalScore:000000}";
            }
        }

        public void TakePlayerDamage(int dmg)
        {

            AddScore(-100);

            if (railCamera != null) StartCoroutine(railCamera.Shake(0.3f, 1.0f));
            if (scoreText != null) StartCoroutine(FlashScoreRed());
        }

        private IEnumerator FlashScoreRed()
        {
            if (scoreText != null) scoreText.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            if (scoreText != null) scoreText.color = Color.white;
        }

        public void UpdateBossHP(float current, float max)
        {
            if (bossHpSlider != null)
            {
                bossHpSlider.gameObject.SetActive(true);
                bossHpSlider.value = current / max;
            }
        }

        public void OnMonolithFixed()
        {
            isPhaseActive = false;
            if (bossHpSlider != null) bossHpSlider.gameObject.SetActive(false);
        }

        private IEnumerator MainGameLoop()
        {
            yield return new WaitForSeconds(2f);


            List<CreditSection> combatSections = new List<CreditSection>();
            CreditSection endSection = null;

            foreach (var section in creditSections)
            {
                if (section.Role == "End") endSection = section;
                else combatSections.Add(section);
            }


            if (combatSections.Count > 0)
            {
                yield return SpawnBossAndBattle(combatSections);
            }


            if (endSection != null)
            {
                string msg = (endSection.Names.Count > 0) ? endSection.Names[0] : "Thank You";
                if (scoreText != null) scoreText.enabled = false;
                yield return ShowEndSequence(msg);
            }

            yield return new WaitForSeconds(3f);
            ReturnToTitle();
        }

        private IEnumerator SpawnBossAndBattle(List<CreditSection> sections)
        {
            railCamera.IsBoost = true;

            float travelTime = 2.5f;
            float timeElapsed = 0f;
            while(timeElapsed < travelTime)
            {
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            railCamera.IsBoost = false;

            float spawnY = 8f;
            GameObject obj = Instantiate(creditPrefab, new Vector3(0, spawnY, 0), Quaternion.identity);

            CreditObject co = obj.GetComponent<CreditObject>();
            co.SetupMonolith(sections);

            yield return new WaitForSeconds(1.0f);

            isPhaseActive = true;


            while(co != null && !co.IsFixed)
            {
                 yield return null;
            }

            isPhaseActive = false;
            yield return new WaitForSeconds(2.0f);
        }

        private IEnumerator ShowEndSequence(string msg)
        {

             ShowCreditName(msg);
             yield return new WaitForSeconds(4.0f);
        }

        private void ReturnToTitle()
        {
            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.FadeOut(1.0f, () => {
                   GameManager.Instance.ChangeState(GameState.Title);
                });
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Title);
            }
        }

        private void SetupUI()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject cObj = new GameObject("CreditCanvas");
                canvas = cObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = cObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                cObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }


            if (bossHpSlider == null)
            {
                GameObject sliderObj = new GameObject("BossHPSlider");
                sliderObj.transform.SetParent(canvas.transform, false);
                bossHpSlider = sliderObj.AddComponent<UnityEngine.UI.Slider>();

                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(sliderObj.transform, false);
                var bgImg = bgObj.AddComponent<UnityEngine.UI.Image>();
                bgImg.color = new Color(0.2f, 0, 0, 0.5f);

                GameObject fillArea = new GameObject("Fill Area");
                fillArea.transform.SetParent(sliderObj.transform, false);
                var areaRect = fillArea.AddComponent<RectTransform>();
                areaRect.anchorMin = Vector2.zero;
                areaRect.anchorMax = Vector2.one;
                areaRect.offsetMin = Vector2.zero;
                areaRect.offsetMax = Vector2.zero;

                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(fillArea.transform, false);
                var fillImg = fillObj.AddComponent<UnityEngine.UI.Image>();
                fillImg.color = Color.red;

                bossHpSlider.targetGraphic = bgImg;
                bossHpSlider.fillRect = fillImg.rectTransform;
                bossHpSlider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
                bossHpSlider.value = 1.0f;
                bossHpSlider.interactable = false;

                RectTransform rt = bossHpSlider.GetComponent<RectTransform>();

                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(0.5f, 0);
                rt.anchoredPosition = new Vector2(0, 0);
                rt.sizeDelta = new Vector2(0, 20);

                bossHpSlider.gameObject.SetActive(false);
            }

            if (scoreText == null)
            {
                GameObject txtObj = new GameObject("ScoreText");
                txtObj.transform.SetParent(canvas.transform, false);
                scoreText = txtObj.AddComponent<TextMeshProUGUI>();
                scoreText.text = "SCORE: 000000";
                scoreText.fontSize = 24;
                scoreText.color = Color.white;
                scoreText.alignment = TextAlignmentOptions.TopLeft;

                RectTransform rt = scoreText.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = new Vector2(20, -20);
                rt.sizeDelta = new Vector2(300, 50);
            }


            GameObject btnObj = new GameObject("ReturnButton");
            btnObj.transform.SetParent(canvas.transform, false);
            var btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            var btn = btnObj.AddComponent<UnityEngine.UI.Button>();

            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1, 1);
            btnRt.anchorMax = new Vector2(1, 1);
            btnRt.pivot = new Vector2(1, 1);
            btnRt.anchoredPosition = new Vector2(-20, -80);
            btnRt.sizeDelta = new Vector2(160, 40);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.text = "RETURN";
            txt.fontSize = 20;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;

            RectTransform txtRt = textObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            btn.onClick.AddListener(() => {
                btn.interactable = false;
                StopAllCoroutines();
                ReturnToTitle();
            });
        }

        public void ShowCreditName(string name)
        {
            if (creditNamePrefab == null) return;
            StartCoroutine(ShowCreditNameRoutine(name));
        }

        private IEnumerator ShowCreditNameRoutine(string name)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) yield break;

            GameObject txtObj = Instantiate(creditNamePrefab, canvas.transform, false);

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = name;
            }


            float duration = 2.0f;
            float t = 0f;
            Vector3 startScale = Vector3.one * 0.5f;
            Vector3 targetScale = Vector3.one * 1.2f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = t / duration;


                float scalePhase = Mathf.SmoothStep(0, 1, Mathf.Min(1, t * 2));
                txtObj.transform.localScale = Vector3.Lerp(startScale, Vector3.one, scalePhase);


                if (progress > 0.5f && tmp != null)
                {
                    float alpha = 1f - ((progress - 0.5f) * 2f);
                    tmp.alpha = alpha;
                }

                yield return null;
            }

            Destroy(txtObj);


        }
    }
}
