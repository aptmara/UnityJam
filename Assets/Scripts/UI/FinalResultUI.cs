using UnityEngine;
using UnityEngine.UI;
using TMPro;
using unityroom.Api;
using naichilab; // Tweet機能用
using UnityJam.Core;

namespace UnityJam.UI
{
    public class FinalResultUI : MonoBehaviour
    {
        [Header("Score Texts")]
        [SerializeField] private TMP_Text round1ScoreText;
        [SerializeField] private TMP_Text round2ScoreText;
        [SerializeField] private TMP_Text round3ScoreText;
        [SerializeField] private TMP_Text totalScoreText;

        [Header("Item List")]
        [SerializeField] private ItemListDisplay totalItemListDisplay;

        [Header("Buttons")]
        [SerializeField] private Button quitButton;
        [SerializeField] private Button tweetButton; // Tweetボタン

        [Header("Unityroom")]
        [Tooltip("unityroomのスコアボードNo")]
        [SerializeField] private int boardNo = 1;
        [Tooltip("スコア送信モード")]
        [SerializeField] private ScoreboardWriteMode writeMode = ScoreboardWriteMode.HighScoreDesc;

        [Header("Tweet Settings")]
        [Tooltip("unityroomのゲームID (URLの最後)")]
        [SerializeField] private string gameId = "コレクライト";

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (GameSessionManager.Instance != null)
            {
                // スコア表示
                SetScoreText(round1ScoreText, GameSessionManager.Instance.GetRoundScore(1));
                SetScoreText(round2ScoreText, GameSessionManager.Instance.GetRoundScore(2));
                SetScoreText(round3ScoreText, GameSessionManager.Instance.GetRoundScore(3));
                
                int totalScore = GameSessionManager.Instance.GetTotalScore();
                SetScoreText(totalScoreText, totalScore);

                 if (totalItemListDisplay != null)
                 {
                     totalItemListDisplay.ShowItems(GameSessionManager.Instance.TotalItems);
                 }

                 // unityroomへスコア送信
                 if (UnityroomApiClient.Instance != null)
                 {
                     UnityroomApiClient.Instance.SendScore(boardNo, totalScore, writeMode);
                 }
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (tweetButton != null)
            {
                tweetButton.onClick.AddListener(OnTweetClicked);
            }
        }

        private void SetScoreText(TMP_Text text, int score)
        {
            if (text != null)
            {
                text.text = $"{score:N0}";
            }
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }

        private void OnTweetClicked()
        {
            int totalScore = 0;
            if (GameSessionManager.Instance != null)
            {
                totalScore = GameSessionManager.Instance.GetTotalScore();
            }

            string text = $"コレクライトで {totalScore:N0} 点を獲得しました！";
            UnityRoomTweet.Tweet(gameId, text, "unityroom", "unity1week");
        }
    }
}
