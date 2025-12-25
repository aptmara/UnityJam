using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UnityJam.UI
{
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private Button returnButton;
        [SerializeField] private TextMeshProUGUI gameOverText;

        private void Start()
        {
            // カーソル解放
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (returnButton != null)
            {
                returnButton.onClick.AddListener(OnReturnClicked);
            }

            // 簡易アニメーション
            if (gameOverText != null)
            {
                gameOverText.alpha = 0f;
                StartCoroutine(FadeInText());
            }
        }

        private System.Collections.IEnumerator FadeInText()
        {
            float duration = 2.0f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                if (gameOverText != null) gameOverText.alpha = t;
                yield return null;
            }
            if (gameOverText != null) gameOverText.alpha = 1f;
        }

        private void OnReturnClicked()
        {
            if (returnButton != null) returnButton.interactable = false;

            if (ScreenFader.Instance != null)
            {
                ScreenFader.Instance.FadeOut(1.0f, () =>
                {
                    GameManager.Instance.ChangeState(GameState.Title);
                });
            }
            else
            {
                GameManager.Instance.ChangeState(GameState.Title);
            }
        }
    }
}
