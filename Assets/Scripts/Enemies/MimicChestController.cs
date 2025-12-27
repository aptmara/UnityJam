using System.Collections;
using UnityEngine;
using UnityJam.Interaction;

namespace UnityJam.Gimmicks
{
    public class MimicChestController : InteractableBase
    {
        [Header("--- 演出 ---")]
        [SerializeField] private GameObject monsterModel;
        [SerializeField] private GameObject boxModel;

        [Header("--- アニメーション ---")]
        [SerializeField] private Animator animator;
        [SerializeField] private string activateTriggerName = "Activate";
        [SerializeField, Min(0f)] private float gameOverDelay = 0.35f;

        [Header("--- Light ---")]
        [SerializeField] private Light mimicLight;

        [SerializeField]
        private Vector3 burstLightOffset = new Vector3(0f, 1.0f, 0f);

        private bool activated;

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

        private void ActivateTrap()
        {
            if (activated) return;
            activated = true;

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
                        GameManager.Instance?.ChangeState(GameState.GameOver);
                    });
            }
            else
            {
                GameManager.Instance?.ChangeState(GameState.GameOver);
            }
        }
    }
}
