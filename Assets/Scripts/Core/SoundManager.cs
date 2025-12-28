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
            else
            {
                // For all other states (Gameplay, Result, Shop, GameOver, etc.), play Game BGM
                // This ensures continuity during result/shop transitions
                PlayBGM(gameBGM);
            }
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;

            // Check if already playing the same clip
            if (bgmSource.isPlaying && bgmSource.clip == clip)
            {
                return;
            }

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.Play();
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
