using UnityEngine;
using UnityEngine.UI;

namespace UnityJam.UI
{
    /// <summary>
    /// ゴール到達メッセージ表示（Unity標準UI Text）。
    /// TMPにしたい場合はフィールドを差し替えるだけで運用可能。
    /// </summary>
    public sealed class GoalMessageView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text messageText;
        [SerializeField] private string defaultMessage = "GOAL!";

        private void Awake()
        {
            Hide();
        }

        public void Show(string message = null)
        {
            if (messageText != null)
            {
                messageText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;
            }

            if (root != null)
            {
                root.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
