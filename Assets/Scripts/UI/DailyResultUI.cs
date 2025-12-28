using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityJam.Core;

namespace UnityJam.UI
{
    public class DailyResultUI : MonoBehaviour
    {
        [Header("Score Info")]
        [SerializeField] private TMP_Text dayScoreText;
        [SerializeField] private TMP_Text totalScoreText;

        [Header("Shortcut")]
        [SerializeField] private TMP_Text shortcutInfoText; // "スキップ不可" or "3Fまでスキップ(200G)"
        [SerializeField] private Button shortcutButton;
        [SerializeField] private TMP_Text shortcutButtonText; // To change text to "Purchased"

        [Header("Navigation")]
        [SerializeField] private Button goToShopButton;

        [Header("Feedback")]
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private Color successColor = Color.cyan;
        [SerializeField] private Color failColor = Color.red;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (feedbackText != null) feedbackText.text = "";

            // スコア表示
            if (GameSessionManager.Instance != null)
            {
                int currentDay = GameSessionManager.Instance.CurrentDayIndex; 
                // CurrentDayIndexはRoundEndでインクリメント済みなので、
                // 表示したいのは「終わったばかりの日」= CurrentDayIndex (1始まりなら CurrentDayIndex)
                // 例: Day1終了 -> Index=1. 表示はDay1の結果。
                // GetDayScore(1) -> Index 0.
                
                int finishedDay = currentDay; // 1始まりの日数
                if (finishedDay < 1) finishedDay = 1;

                int dayScore = GameSessionManager.Instance.GetDayScore(finishedDay);
                if (dayScoreText != null) dayScoreText.text = $"{finishedDay}日目 スコア: {dayScore:N0}";

                if (totalScoreText != null)
                {
                    totalScoreText.text = $"合計スコア: {GameSessionManager.Instance.GetTotalScore():N0}";
                }

                UpdateShortcutUI();
            }

            if (shortcutButton != null)
            {
                shortcutButton.onClick.AddListener(OnShortcutBuy);
            }

            if (goToShopButton != null)
            {
                goToShopButton.onClick.AddListener(OnGoToShopClicked);
            }
        }

        private void UpdateShortcutUI()
        {
            if (GameSessionManager.Instance == null) return;
            if (shortcutInfoText == null) return;

            int lastReached = GameSessionManager.Instance.LastReachedFloor;

            // 固定コスト: 200スタート (Day2=200, Day3=250...)
            // CurrentDayIndexはDay1終了後=1 (Day2開始前)
            int currentDayIndex = GameSessionManager.Instance.CurrentDayIndex;
            int baseCost = 200;
            // Day1終了時(Idx1) -> Day2のショートカット -> Cost 200
            // Day2終了時(Idx2) -> Day3のショートカット -> Cost 250
            int cost = baseCost + (Mathf.Max(0, currentDayIndex - 1) * 50);

            if (lastReached > 1)
            {
                shortcutInfoText.text = $"{lastReached}Fから開始 (費用:{cost})";
                if (shortcutButton != null) shortcutButton.interactable = true;
            }
            else
            {
                shortcutInfoText.text = "スキップ不可";
                if (shortcutButton != null) shortcutButton.interactable = false;
            }
        }

        private void OnShortcutBuy()
        {
            if (GameSessionManager.Instance == null) return;
            if (Inventory.Instance == null) return;

            int lastReached = GameSessionManager.Instance.LastReachedFloor;
            if (lastReached <= 1) return;

            int currentDayIndex = GameSessionManager.Instance.CurrentDayIndex;
            int baseCost = 200;
            int cost = baseCost + (Mathf.Max(0, currentDayIndex - 1) * 50);

            int currentScore = Inventory.Instance.TotalScore;

            if (currentScore >= cost)
            {
                // Pay
                Inventory.Instance.SpendScore(cost);
                
                // Set Start Floor
                GameSessionManager.Instance.NextDayStartFloor = lastReached;

                // Update UI
                if (shortcutButton != null)
                {
                    shortcutButton.interactable = false;
                    if(shortcutButtonText != null) shortcutButtonText.text = "購入済み";
                }
                
                // Update Total Score Display
                if (totalScoreText != null)
                {
                    // Total Score (Record) remains the same even if money is spent
                    totalScoreText.text = $"合計スコア: {GameSessionManager.Instance.GetTotalScore():N0}";
                }

                // Success Feedback
                if (feedbackText != null)
                {
                    feedbackText.text = "購入しました！";
                    feedbackText.color = successColor;
                }
            }
            else
            {
                // Not enough score feedback
                Debug.Log("Not enough score");
                StartCoroutine(ShowErrorFeedback("スコアが足りません！"));
            }
        }

        private IEnumerator ShowErrorFeedback(string message)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = failColor;
                yield return new WaitForSeconds(2.0f);
                feedbackText.text = "";
            }
        }

        private void OnGoToShopClicked()
        {
            // Transition to Shop
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Shop);
            }
        }
    }
}
