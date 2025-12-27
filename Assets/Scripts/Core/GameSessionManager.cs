using System.Collections.Generic;
using UnityEngine;
using UnityJam.Items;

namespace UnityJam.Core
{
    /// <summary>
    /// 3回勝負のセッション全体を管理するマネージャー
    /// </summary>
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        public const int MaxRounds = 3;

        // 現在のラウンド（0始まり: 0=1回目, 1=2回目...）
        public int CurrentRoundIndex { get; private set; } = 0;

        // 各ラウンドのスコア履歴
        public List<int> RoundScores { get; private set; } = new List<int>();

        // セッション全体で取得したアイテムの総数
        public Dictionary<ItemMaster, int> TotalItems { get; private set; } = new Dictionary<ItemMaster, int>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// ラウンドの結果を記録し、次のラウンドへ進む準備をする
        /// </summary>
        public void RegisterRoundResult(int score, Dictionary<ItemMaster, int> items)
        {
            Debug.Log($"[GameSessionManager] Registering Round {CurrentRoundIndex + 1} Result. Score: {score}");

            // スコア記録
            RoundScores.Add(score);

            // アイテム集計
            foreach (var kvp in items)
            {
                ItemMaster item = kvp.Key;
                int count = kvp.Value;

                if (TotalItems.ContainsKey(item))
                    TotalItems[item] += count;
                else
                    TotalItems[item] = count;
            }

            // ラウンド進行
            CurrentRoundIndex++;
        }

        /// <summary>
        /// 全ラウンド終了かどうか
        /// </summary>
        public bool IsSessionFinished()
        {
            return CurrentRoundIndex >= MaxRounds;
        }

        /// <summary>
        /// セッションをリセット（タイトルに戻る時など）
        /// </summary>
        public void ResetSession()
        {
            CurrentRoundIndex = 0;
            RoundScores.Clear();
            TotalItems.Clear();
            Debug.Log("[GameSessionManager] Session Reset");
        }

        /// <summary>
        /// 全ラウンドの合計スコアを取得
        /// </summary>
        public int GetTotalScore()
        {
            int total = 0;
            foreach (var s in RoundScores) total += s;
            return total;
        }

        /// <summary>
        /// 特定のラウンドのスコアを取得 (1始まりのラウンド番号を指定)
        /// </summary>
        public int GetRoundScore(int roundNumber)
        {
            int index = roundNumber - 1;
            if (index >= 0 && index < RoundScores.Count)
            {
                return RoundScores[index];
            }
            return 0;
        }
    }
}
