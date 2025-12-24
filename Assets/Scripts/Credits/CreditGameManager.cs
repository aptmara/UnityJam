using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace UnityJam.Credits
{
    public class CreditGameManager : MonoBehaviour
    {
        public static CreditGameManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float scrollSpeed = 1f;
        [SerializeField] private float startY = 10f; // 画面上部から開始
        [SerializeField] private float spacing = 3f; // アイテム間の間隔
        [SerializeField] private float returnToTitleDelay = 3f;

        [Header("UI")]
        [SerializeField] private Button returnButton;
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Prefabs")]
        [SerializeField] private GameObject creditTextPrefab; // Must have CreditObject attached
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject enemyBulletPrefab;
        [SerializeField] private Transform contentRoot; // Parent for text

        [Header("Data")]
        [TextArea(3, 10)]
        [SerializeField] private List<string> creditTextData = new List<string>() 
        {
            "[Director]\nLegendary - Yamakawa"
        };

        private List<CreditObject> activeObjects = new List<CreditObject>();
        private bool isScrolling = true;
        private GameObject playerInstance;
        private int currentScore = 0;
        
        // Shake
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0.1f;
        private Vector3 initialCamPos;
        private Transform cameraTransform;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            SetupCamera();
            CreateStarfield();
            SetupUI();
            
            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.FadeIn();
            }
            StartCoroutine(Routine());
        }
        
        private void Update()
        {
            if (shakeDuration > 0)
            {
                cameraTransform.localPosition = initialCamPos + Random.insideUnitSphere * shakeMagnitude;
                shakeDuration -= Time.deltaTime;
            }
            else if (cameraTransform != null)
            {
                cameraTransform.localPosition = initialCamPos;
            }
        }

        public void AddScore(int amount)
        {
            currentScore += amount;
            if (scoreText != null) scoreText.text = $"SCORE: {currentScore:N0}";
        }

        public void TriggerShake(float duration = 0.2f, float magnitude = 0.2f)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }

        private void CreateStarfield()
        {
            GameObject stars = new GameObject("Starfield");
            stars.transform.SetParent(transform);
            stars.transform.position = new Vector3(0, 10, 5);
            stars.transform.rotation = Quaternion.Euler(90, 0, 0); // Emit downwards
            
            var ps = stars.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 10f;
            main.startSpeed = 10f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            main.startColor = new Color(1, 1, 1, 0.5f);
            main.maxParticles = 1000;
            
            var emission = ps.emission;
            emission.rateOverTime = 50;
            
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20, 1, 1);
            
            var renderer = stars.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        }

        private void SetupUI()
        {
            if (returnButton == null)
            {
                // 最低限のUIを作成
                GameObject canvasObj = new GameObject("CreditCanvas");
                canvasObj.transform.SetParent(transform);
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                GameObject btnObj = new GameObject("ReturnButton");
                btnObj.transform.SetParent(canvasObj.transform);
                Image img = btnObj.AddComponent<Image>();
                img.color = new Color(0, 0, 0, 0.5f);
                
                returnButton = btnObj.AddComponent<Button>();
                
                RectTransform rt = btnObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.one;
                rt.anchorMax = Vector2.one;
                rt.pivot = Vector2.one;
                rt.anchoredPosition = new Vector2(-20, -20);
                rt.sizeDelta = new Vector2(160, 50);

                // テキスト
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(btnObj.transform);
                TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = "Return";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 24;
                tmp.color = Color.white;
                
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = Vector2.zero;
                textRt.anchorMax = Vector2.one;
                textRt.offsetMin = Vector2.zero;
                textRt.offsetMax = Vector2.zero;
                
                // Score Text
                GameObject scoreObj = new GameObject("ScoreText");
                scoreObj.transform.SetParent(canvasObj.transform);
                scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
                scoreText.text = "SCORE: 0";
                scoreText.fontSize = 36;
                scoreText.color = Color.cyan;
                scoreText.alignment = TextAlignmentOptions.Left;
                
                RectTransform scoreRt = scoreObj.GetComponent<RectTransform>();
                scoreRt.anchorMin = new Vector2(0, 1);
                scoreRt.anchorMax = new Vector2(0, 1);
                scoreRt.pivot = new Vector2(0, 1);
                scoreRt.anchoredPosition = new Vector2(20, -20);
                scoreRt.sizeDelta = new Vector2(400, 50);
            }

            returnButton.onClick.AddListener(() => 
            {
                isScrolling = false;
                ReturnToTitle();
            });
        }

        private void SetupCamera()
        {
            GameObject camObj = new GameObject("CreditCamera");
            camObj.transform.SetParent(transform); 
            camObj.transform.position = new Vector3(0, 0, -10f);
            
            cameraTransform = camObj.transform;
            initialCamPos = cameraTransform.localPosition;

            Camera cam = camObj.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.orthographic = true;
            cam.orthographicSize = 5f;

            camObj.AddComponent<AudioListener>();
        }

        private IEnumerator Routine()
        {
            if (playerPrefab != null)
            {
                Vector3 spawnPos = new Vector3(0, -4.5f, 0);
                playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            }

            // リストを結合
            string combinedText = string.Join("\n", creditTextData);
            List<CreditSection> sections = CreditTextParser.Parse(combinedText);
            float currentY = startY;

            foreach (var section in sections)
            {
                // Role (Header)
                CreateTextObject(section.RoleName, true, 0f, ref currentY, false);
                currentY += spacing;

                foreach (var entry in section.Entries)
                {
                    // Description (Small, Center)
                    if (!string.IsNullOrEmpty(entry.Description))
                    {
                        CreateTextObject(entry.Description, true, 0f, ref currentY, false);
                        currentY += spacing * 0.8f;
                    }

                    // Title & Name (Enemies)
                    if (!string.IsNullOrEmpty(entry.Title))
                    {
                        CreateTextObject(entry.Title, false, -4f, ref currentY, true); 
                        CreateTextObject(entry.Name, false, 4f, ref currentY, true); 
                    }
                    else
                    {
                        CreateTextObject(entry.Name, false, 0f, ref currentY, true);
                    }
                    currentY += spacing * 1.5f; // More space after entry
                }
                
                currentY += spacing;
            }
            
            // Main Loop
            while (isScrolling)
            {
                if (contentRoot != null)
                {
                    contentRoot.position += Vector3.down * scrollSpeed * Time.deltaTime;
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    isScrolling = false;
                    ReturnToTitle();
                    yield break;
                }

                // Check end
                // last object Y is roughly currentY (max Y calculated)
                // ContentRoot.y + currentY < -10 (roughly bottom of screen?)
                // Actually if ContentRoot moves DOWN (negative Y), and items are at POSITIVE Y relative to it.
                // Item world Y = ContentRoot.y + Item.localY.
                // Last item is at highest localY (~currentY).
                // We want Last Item to pass bottom of screen (-6f).
                // So ContentRoot.y + currentY < -6f -> ContentRoot.y < -6f - currentY.
                
                if (contentRoot.position.y < -(currentY + 15f))
                {
                    isScrolling = false;
                }

                yield return null;
            }

            ShowFinalAlignment();
        }

        private void ReturnToTitle()
        {
            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.FadeOut(1f, () => 
                {
                    GameManager.Instance.ChangeState(GameState.Title);
                });
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Title);
            }
        }

        private void ShowFinalAlignment()
        {
            // 現在のものをクリア
            foreach(Transform child in contentRoot)
            {
                Destroy(child.gameObject);
            }
            activeObjects.Clear();
            contentRoot.position = Vector3.zero; // ルートをリセット
            contentRoot.rotation = Quaternion.identity;

            string combinedText = string.Join("\n", creditTextData);
            List<CreditSection> sections = CreditTextParser.Parse(combinedText);
            List<string> allTexts = new List<string>();

            // リストを平坦化
            foreach (var section in sections)
            {
                allTexts.Add(section.RoleName);
                foreach (var entry in section.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.Title)) allTexts.Add(entry.Title);
                    allTexts.Add(entry.Name);
                }
            }

            // 単純なグリッド計算
            int cols = 4;
            // int rows = Mathf.CeilToInt((float)allTexts.Count / cols);
            
            float startX = -6f;
            float startY = 4f;
            float gapX = 4f;
            float gapY = 1.5f;

            for (int i = 0; i < allTexts.Count; i++)
            {
                int r = i / cols;
                int c = i % cols;

                Vector3 pos = new Vector3(startX + (c * gapX), startY - (r * gapY), 0);
                
                // 直接作成（絶対座標を簡略化するためヘルパーをスキップ）
                if (creditTextPrefab != null)
                {
                    GameObject obj = Instantiate(creditTextPrefab, contentRoot);
                    obj.transform.localPosition = pos;
                    var co = obj.GetComponent<CreditObject>();
                    if (co != null) co.Setup(allTexts[i], false); 
                }
            }
        }

        private void CreateTextObject(string text, bool isTitle, float xPos, ref float yPos, bool isEnemy = false)
        {
            if (creditTextPrefab != null && contentRoot != null)
            {
                GameObject obj = Instantiate(creditTextPrefab, contentRoot);
                obj.transform.localPosition = new Vector3(xPos, yPos, 0); 
                
                var creditObj = obj.GetComponent<CreditObject>();
                if (creditObj != null)
                {
                    // Pass player info for enemy behavior
                    Transform pTransform = playerInstance != null ? playerInstance.transform : null;
                    creditObj.Setup(text, isTitle, isEnemy, pTransform, enemyBulletPrefab);
                    activeObjects.Add(creditObj);
                }
            }
        }
    }
}
