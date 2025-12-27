using UnityEngine;
using TMPro;
using UnityJam.Core;

namespace UnityJam.UI
{
    /// <summary>
    /// インベントリのスコアを表示するためのクラス
    /// </summary>
    public class ScoreDisplay : MonoBehaviour
    {
        public enum DisplayType
        {
            CurrentScore,
            HighScore
        }

        [SerializeField] private TMP_Text scoreText;

        [Header("Options")]
        [Tooltip("表示するスコアの種類を選択")]
        [SerializeField] private DisplayType displayType = DisplayType.CurrentScore;
        
        // ハイスコア保存時の固定キー
        private const string HIGH_SCORE_KEY = "HighScore";

        private void Update()
        {
            // Inventoryのインスタンスが存在し、テキストコンポーネントが割り当てられている場合のみ更新
            if (Inventory.Instance != null && scoreText != null)
            {
                int currentScore = Inventory.Instance.TotalScore; // 引っ張ってくる

                // ハイスコアの更新と取得
                int highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
                if (currentScore > highScore)
                {
                    highScore = currentScore;
                    PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
                    PlayerPrefs.Save(); // 確実に保存
                }

                // 表示の分岐
                if (displayType == DisplayType.CurrentScore)
                {
                    scoreText.text = $"Score:{currentScore:N0}";
                }
                else
                {
                    scoreText.text = $"HighScore:{highScore:N0}";
                }
            }
        }
    }
}
