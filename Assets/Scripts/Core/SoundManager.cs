using UnityEngine;
using UnityJam.Core; // Namespace for structure

namespace UnityJam.Core
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Source")]
        [SerializeField] private AudioSource bgmSource;

        [Header("BGM Clips")]
        [SerializeField] private AudioClip titleBGM;
        [SerializeField] private AudioClip creditBGM; // Added for Credits
        [SerializeField] private AudioClip gameBGM;   // Restored
        [Header("SE")]
        [SerializeField] private AudioSource seSource;
        [SerializeField] private AudioClip uiClickSE;

        [Header("BGM Loop Setting (Game BGM Only)")]
        // 00:06:21.10 = 6 + 21/30 + 10/(30*80) = 6.704166f
        [SerializeField] private float gameLoopStartTime = 6.704166f;
        // 01:00:00.00 = 60.0f
        [SerializeField] private float gameLoopEndTime = 60.0f;
        [SerializeField] private float gameBGMFadeDuration = 2.0f;

        private Coroutine fadeCoroutine;

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
            if (bgmSource == null)
            {
                bgmSource = GetComponent<AudioSource>();
                if (bgmSource == null)
                {
                    bgmSource = gameObject.AddComponent<AudioSource>();
                }
            }
            bgmSource.loop = true;

            if (seSource == null)
            {
                // Create a separate source for SE if not assigned
                var childObj = new GameObject("SE_Source");
                childObj.transform.parent = transform;
                seSource = childObj.AddComponent<AudioSource>();
            }

            // Subscribe to state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                // Initial state check
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void Update()
        {
            // BGMのループ制御 (Ingame BGMのみ)
            if (bgmSource != null && bgmSource.isPlaying && bgmSource.clip == gameBGM)
            {
                if (bgmSource.time >= gameLoopEndTime)
                {
                    bgmSource.time = gameLoopStartTime;
                }
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
            if (state == GameState.Title)
            {
                PlayBGM(titleBGM);
            }
            else if (state == GameState.Credits)
            {
                PlayBGM(creditBGM);
            }
            else if (state == GameState.Loading)
            {
                // ロード中は止めない、あるいはそのまま
            }
            else
            {
                // For all other states (Gameplay, Result, Shop, GameOver, etc.), play Game BGM
                // This ensures continuity during result/shop transitions

                // もし既に再生中でないならフェードインで開始
                if (bgmSource.clip != gameBGM)
                {
                    PlayBGM(gameBGM, gameBGMFadeDuration);
                }
            }
        }

        public void PlayBGM(AudioClip clip, float fadeDuration = 0f)
        {
            if (clip == null) return;

            // Check if already playing the same clip
            if (bgmSource.isPlaying && bgmSource.clip == clip)
            {
                return;
            }

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

            bgmSource.Stop();
            bgmSource.clip = clip;

            if (fadeDuration > 0f)
            {
                bgmSource.volume = 0f;
                bgmSource.Play();
                fadeCoroutine = StartCoroutine(FadeInRoutine(fadeDuration));
            }
            else
            {
                bgmSource.volume = 1f; // デフォルトボリューム（必要なら変数化）
                bgmSource.Play();
            }
        }

        private System.Collections.IEnumerator FadeInRoutine(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmSource.volume = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            bgmSource.volume = 1f;
            fadeCoroutine = null;
        }

        public void PlayUIClick()
        {
            if (uiClickSE != null && seSource != null)
            {
                seSource.PlayOneShot(uiClickSE);
            }
        }
    }
}
