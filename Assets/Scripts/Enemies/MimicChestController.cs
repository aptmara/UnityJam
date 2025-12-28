using System.Collections;
using UnityEngine;
using UnityJam.Interaction;

namespace UnityJam.Gimmicks
{
    public class MimicChestController : InteractableBase
    {
        [Header("--- 演出 (モデル) ---")]
        [SerializeField] private GameObject monsterModel;
        [SerializeField] private GameObject boxModel;

        [Header("--- 演出（待機中の揺れ） ---")]
        [Tooltip("揺れる間隔（秒）。この秒数ごとにガタガタします")]
        [SerializeField] private float shakeInterval = 3.0f;

        [Tooltip("一度の揺れの長さ（秒）")]
        [SerializeField] private float shakeDuration = 0.5f;

        [Tooltip("揺れの強さ（角度）")]
        [SerializeField] private float shakeStrength = 2.0f;

        [Header("--- アニメーション ---")]
        [SerializeField] private Animator animator;
        [SerializeField] private string activateTriggerName = "Activate";
        [SerializeField, Min(0f)] private float gameOverDelay = 0.35f;

        [Header("--- Light ---")]
        [SerializeField] private Light mimicLight;

        [SerializeField]
        private Vector3 burstLightOffset = new Vector3(0f, 1.0f, 0f);

        private bool activated;
        private Quaternion initialRotation; // 元の角度を保存
        private Coroutine shakeCoroutine;

        protected override void OnInteractCompleted()
        {
            ActivateTrap();
        }

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            // 初期角度を保存
            initialRotation = transform.localRotation;

            // 揺れ演出を開始
            shakeCoroutine = StartCoroutine(IdleShakeRoutine());
        }

        // 待機中の揺れループ
        private IEnumerator IdleShakeRoutine()
        {
            while (!activated)
            {
                // 次の揺れまで待機
                yield return new WaitForSeconds(shakeInterval);

                if (activated) yield break;

                // ガタガタ揺らす
                float elapsed = 0f;
                while (elapsed < shakeDuration)
                {
                    if (activated) yield break;

                    // ランダムに少し傾ける
                    float x = Random.Range(-1f, 1f) * shakeStrength;
                    float z = Random.Range(-1f, 1f) * shakeStrength;

                    // Y軸（向き）は変えず、XとZでガタつかせる
                    transform.localRotation = initialRotation * Quaternion.Euler(x, 0, z);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // 揺れ終わったらピタッと戻す
                transform.localRotation = initialRotation;
            }
        }

        private void ActivateTrap()
        {
            if (activated) return;
            activated = true;

            // 揺れを停止して角度をリセット
            if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
            transform.localRotation = initialRotation;

            // 1. 見た目切替
            if (boxModel != null) boxModel.SetActive(false);
            if (monsterModel != null) monsterModel.SetActive(true);

            // 2. 覚醒アニメ
            if (animator != null && !string.IsNullOrEmpty(activateTriggerName))
            {
                animator.ResetTrigger(activateTriggerName);
                animator.SetTrigger(activateTriggerName);
            }

            // 3. 覚醒ライト演出（宝箱と同等）
            PlayBurstLight();

            // 4. ゲームオーバー遅延
            if (gameOverDelay > 0f)
            {
                StartCoroutine(GameOverAfterDelay(gameOverDelay));
            }
            else
            {
                DoGameOver();
            }
        }

        private void PlayBurstLight()
        {
            if (mimicLight == null) return;
            mimicLight.enabled = true;
        }


        private IEnumerator GameOverAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            DoGameOver();
        }

        private void DoGameOver()
        {
            UnityJam.Player.PlayerRegistry.DeathHandler?.KillAndHide();

            if (UnityJam.Cameras.DeathCameraFocus.Instance != null)
            {
                UnityJam.Cameras.DeathCameraFocus.Instance.PlayFocus(
                    transform,
                    1.25f,
                    () =>
                    {
                GameManager.Instance?.HandleDayFailed();
                    });
            }
            else
            {
                GameManager.Instance?.ChangeState(GameState.GameOver);
            }
        }
    }
}
