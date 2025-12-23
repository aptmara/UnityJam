using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityJam.Interaction
{
    /// <summary>
    /// 全インタラクトオブジェクトの親クラス。
    /// 「長押し判定」と「完了時の通知」の機能だけを持ちます。
    /// </summary>
    public abstract class InteractableBase : MonoBehaviour
    {
        [Header("--- Interact Settings ---")]
        [Tooltip("完了に必要な長押し時間（秒）")]
        [SerializeField] protected float requiredHoldTime = 1.0f;

        // 現在の蓄積時間（外部から確認可能だが、書き込みはここだけ）
        public float CurrentHoldTime { get; private set; } = 0f;

        // 進行度（0.0 〜 1.0）UI表示などに便利
        public float Progress => Mathf.Clamp01(CurrentHoldTime / requiredHoldTime);

        private int lastInteractFrame = -1;
        private bool isCompleted = false;

        /// <summary>
        /// プレイヤーから毎フレーム呼ばれる関数
        /// 呼ばれている間だけ秒数を進めます。
        /// </summary>
        public bool AddInteractTime()
        {
            if (isCompleted) return false;

            // 最終インタラクトフレームを更新
            lastInteractFrame = Time.frameCount;

            // 時間を進める
            CurrentHoldTime += Time.deltaTime;

            // 規定時間を超えたら
            if (CurrentHoldTime > requiredHoldTime)
            {
                CurrentHoldTime = requiredHoldTime;
                isCompleted = true;

                // 子クラスごとの処理を実行
                OnInteractCompleted();

                return true;
            }

            return false;
        }


        private void Update()
        {
            // もし「1フレームでもインタラクトが途切れたら」時間をリセットする処理
            // （長押しを中断したら0に戻す場合）
            if (!isCompleted && Time.frameCount > lastInteractFrame + 1)
            {
                if (CurrentHoldTime > 0)
                {
                    // 減衰させるか、即0にするか。今回は「即リセット」にします
                    CurrentHoldTime = 0f;
                }
            }
        }

        // 子クラス（宝箱など）が必ず実装しなければならない「完了時の処理」
        protected abstract void OnInteractCompleted();

        // ギミックを再利用可能にする場合のリセット処理
        public virtual void ResetGimmick()
        {
            isCompleted = false;
            CurrentHoldTime = 0f;
        }
    }
}
