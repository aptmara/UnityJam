using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

using UnityEngine.Rendering.Universal;

namespace UnityJam.Effects
{
    /// <summary>
    /// Global Volume の Bloom エフェクトを制御するクラス。
    /// URP想定。
    /// </summary>
    public sealed class BloomBurstController : MonoBehaviour
    {
#if UNITY_RENDER_PIPELINES_UNIVERSAL
        [Header("--- Target Volume ---")]
        [Tooltip("制御対象の Volume コンポーネント（未設定なら同一GOから自動取得）")]
        [SerializeField] private Volume targetVolume;

        [Header("--- Burst Settings ---")]
        [Tooltip("バースト時の Bloom 強度")]
        [SerializeField] private float burstIntensity = 2.0f;

        [Tooltip("バーストの持続時間（立ち上がり）")]
        [SerializeField] private float burstDuration = 0.35f;

        private Coroutine burstCoroutine;

        private Bloom bloom;
        private float originalIntensity;
        private bool hasOriginal;

        private void Awake()
        {
            // Inspector未設定でも動くように自動取得（Prefab運用の手戻り防止）
            if (targetVolume == null)
            {
                targetVolume = GetComponent<Volume>();
            }

            if (targetVolume == null)
            {
                Debug.LogWarning("BloomBurstController: Volume が見つかりません（同一GameObjectに Volume を付けてください）。", this);
                return;
            }

            if (targetVolume.profile == null)
            {
                Debug.LogWarning("BloomBurstController: Volume Profile が未設定です。", this);
                return;
            }

            if (!targetVolume.profile.TryGet(out bloom) || bloom == null)
            {
                Debug.LogWarning("BloomBurstController: Volume Profile に Bloom が見つかりません。", this);
                return;
            }

            originalIntensity = bloom.intensity.value;
            hasOriginal = true;
        }

        public void PlayBurst()
        {
            if (bloom == null || !hasOriginal)
            {
                Debug.LogWarning("BloomBurstController: Bloom が初期化できていません（Volume/Bloom設定を確認）。", this);
                return;
            }

            if (burstCoroutine != null)
            {
                StopCoroutine(burstCoroutine);
            }

            burstCoroutine = StartCoroutine(BurstRoutine());

        }

        private IEnumerator BurstRoutine()
        {
            float t = 0f;

            // 立ち上がり
            while (t < burstDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / burstDuration);

                // EaseOut
                float eased = 1f - Mathf.Pow(1f - k, 3f);
                bloom.intensity.value = Mathf.Lerp(originalIntensity, burstIntensity, eased);

                yield return null;
            }

            // 戻し
            t = 0f;
            float backDuration = burstDuration * 0.8f;

            while (t < backDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / backDuration);

                // EaseIn
                float eased = k * k;
                bloom.intensity.value = Mathf.Lerp(burstIntensity, originalIntensity, eased);

                yield return null;
            }

            bloom.intensity.value = originalIntensity;
            burstCoroutine = null;
        }
#endif
    }
}
