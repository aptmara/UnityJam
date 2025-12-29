using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityJam.UI
{
    /// <summary>
    /// インタラクト中（宝箱の開封や脱出など）の進行度を表示するUI。
    /// GameManagerによって生成・制御されます。
    /// </summary>
    public class InteractionUI : MonoBehaviour
    {
        [Header("--- UI Components ---")]
        [Tooltip("進行度を表示するためのスライダー（任意）。ImageのFillを使いたい場合は下のfillImageを設定してください。")]
        [SerializeField] private Slider progressSlider;

        [Tooltip("進行度を表示するためのImage（Image TypeをFilledにしてください）。Sliderの代わりにこれを使うことも可能です。")]
        [SerializeField] private Image fillImage;

        [Tooltip("UI全体のCanvasGroup（フェード用）。なければSetActiveで切り替えます。")]
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            // 初期状態は非表示
            Hide();
        }

        /// <summary>
        /// 進行度 (0.0 〜 1.0) を更新して表示します。
        /// </summary>
        public void SetProgress(float progress)
        {
            // まだ表示されていなければ表示する
            if (!IsVisible())
            {
                Show();
            }

            // スライダーがあれば更新（スライダーはそのまま0でOK）
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            // Fill Imageがあれば更新
            if (fillImage != null)
            {
                // 進行度が0（インタラクト開始前・ホバー中）または完了時は、画像そのまま（Fill 1.0）を表示する
                // ただし完了時(1.0)は1.0なので、0のときだけ特殊対応する
                if (progress <= 0f)
                {
                    fillImage.fillAmount = 1.0f;
                }
                else
                {
                    fillImage.fillAmount = progress;
                }
            }
        }

        /// <summary>
        /// UIを表示します。
        /// </summary>
        public void Show()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false; // 操作の邪魔にならないように
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// UIを非表示にします。
        /// </summary>
        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private bool IsVisible()
        {
            if (canvasGroup != null)
            {
                return canvasGroup.alpha > 0f;
            }
            return gameObject.activeSelf;
        }
    }
}
