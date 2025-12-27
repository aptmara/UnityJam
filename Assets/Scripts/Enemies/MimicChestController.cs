using System.Collections;
using UnityEngine;
using UnityJam.Interaction;

namespace UnityJam.Gimmicks
{
    public class MimicChestController : InteractableBase
    {
        [Header("--- 演出 ---")]
        [Tooltip("正体を現した時のモデル（モンスターの姿など）")]
        [SerializeField] private GameObject monsterModel;

        [Tooltip("擬態中のモデル（普通の宝箱）")]
        [SerializeField] private GameObject boxModel;

        [Header("--- アニメーション ---")]
        [Tooltip("このオブジェクト（または子）に付いている Animator。未設定なら子から自動取得します。")]
        [SerializeField] private Animator animator;

        [Tooltip("覚醒アニメを再生するTrigger名")]
        [SerializeField] private string activateTriggerName = "Activate";

        [Tooltip("覚醒後にゲームオーバー処理を行うまでの待ち時間（アニメを見せたい場合）")]
        [SerializeField, Min(0f)] private float gameOverDelay = 0.35f;

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
            if (activated)
            {
                return;
            }

            activated = true;

            Debug.Log("<color=red>ミミック覚醒！！うぎゃぁぁぁっ！！ぶち56された！！GAME OVER</color>");

            // 1. 姿を変える（必要な場合だけ）
            if (boxModel != null) boxModel.SetActive(false);
            if (monsterModel != null) monsterModel.SetActive(true);

            // 2. アニメーション再生（Animatorがある場合）
            if (animator != null && !string.IsNullOrEmpty(activateTriggerName))
            {
                animator.ResetTrigger(activateTriggerName);
                animator.SetTrigger(activateTriggerName);
            }

            // 3. ゲームオーバー（暫定）
            // ここは本来 GameFlow に通知するのが正道（別Sceneにある前提なので、後でイベント/State連携に置換推奨）
            if (gameOverDelay > 0f)
            {
                StartCoroutine(GameOverAfterDelay(gameOverDelay));
            }
            else
            {
                DoGameOver();
            }
        }

        private IEnumerator GameOverAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            DoGameOver();
        }

        private void DoGameOver()
        {
            // 暫定：プレイヤー破壊（Find常用は禁止なので、後でGameFlow通知に差し替える）
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Destroy(player);
            }
        }
    }
}
