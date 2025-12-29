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
        [SerializeField] private TMP_Text day1ScoreText;
        [SerializeField] private TMP_Text day2ScoreText;
        [SerializeField] private TMP_Text day3ScoreText;
        [SerializeField] private TMP_Text totalScoreText;



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
        [SerializeField] private string gameId = "unity-jam-project"; // デフォルト値あるいは空

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (GameSessionManager.Instance != null)
            {
                // スコア表示
                SetScoreText(day1ScoreText, "Day 1 : ", GameSessionManager.Instance.GetDayScore(1), true);
                SetScoreText(day2ScoreText, "Day 2 : ", GameSessionManager.Instance.GetDayScore(2), true);
                SetScoreText(day3ScoreText, "Day 3 : ", GameSessionManager.Instance.GetDayScore(3), true);

                int totalScore = GameSessionManager.Instance.GetTotalScore();
                SetScoreText(totalScoreText, "Total Score : ", totalScore, false);


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

        private void SetScoreText(TMP_Text text, string label, int score, bool hideIfZero)
        {
            if (text != null)
            {
                if (hideIfZero && score <= 0)
                {
                    // スコアが0の場合表示しない（失敗によるスキップ含む）
                    text.gameObject.SetActive(false);
                }
                else
                {
                    text.gameObject.SetActive(true);
                    text.text = $"{label}{score:N0}";
                }
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
