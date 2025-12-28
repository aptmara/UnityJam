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

        public const int MaxDays = 3; // 3日間
        public const int MaxFloors = 4; // 各日4階層

        // 現在の日数（0始まり: 0=1日目, 1=2日目...）
        public int CurrentDayIndex { get; private set; } = 0;

        // 現在の階層（1始まり: 1=1階, 2=2階...）
        public int CurrentFloor { get; private set; } = 1;

        // 前日に到達した階層（ショートカット可能上限）
        public int LastReachedFloor { get; private set; } = 1;

        // 次の日の開始階層（ショップで購入して設定）
        public int NextDayStartFloor { get; set; } = 1;

        // 各ラウンド(日)のスコア履歴
        public List<int> DayScores { get; private set; } = new List<int>();

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
        /// 1日の終了（5階層クリア、または脱出）時の処理
        /// </summary>
        public void RegisterDayResult(int score, Dictionary<ItemMaster, int> items)
        {
            // 重複登録チェック: 既に今日のスコアが登録済みなら無視
            if (DayScores.Count > CurrentDayIndex)
            {
                Debug.LogWarning($"[GameSessionManager] Day {CurrentDayIndex + 1} result already registered. Ignoring duplicate call.");
                return;
            }
            
            Debug.Log($"[GameSessionManager] Registering Day {CurrentDayIndex + 1} Result. Score: {score}");

            // スコア記録
            DayScores.Add(score);

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

            // 到達階層を記録 (クリアしてれば5+1=6になるかもしれないし、脱出ならその階)
            // CurrentFloorが5でクリアした場合、次は5までスキップできるべきか？
            // 仕様: 「前日に到達した階層まで」 -> クリア時は6階扱いだと変だが、最大5階。
            // 5階クリア = Reached 5.
            // もしCurrentFloorが5で、ProceedToNextFloorが呼ばれる前にここに来るなら5。
            // MaxFloorsでキャップする。
            LastReachedFloor = Mathf.Min(CurrentFloor, MaxFloors);
            
            // 次の日へ
            CurrentDayIndex++;
            
            // 階層リセットは StartNextDay で NextDayStartFloor を使うため、ここでは一旦1に戻すか、
            // あるいは StartNextDay で上書きされるので気にしないか。
            // デフォルトは1
            NextDayStartFloor = 1;
            CurrentFloor = 1; 
        }

        /// <summary>
        /// 次の日の開始処理（GameManagerから呼ばれる）
        /// </summary>
        public void StartNextDay()
        {
            CurrentFloor = NextDayStartFloor;
            Debug.Log($"[GameSessionManager] Starting Day {CurrentDayIndex + 1} at Floor {CurrentFloor}");
        }

        /// <summary>
        /// 次の階層へ進む
        /// </summary>
        public void ProceedToNextFloor()
        {
            CurrentFloor++;
            Debug.Log($"[GameSessionManager] Proceeding to Floor {CurrentFloor}");
        }

        /// <summary>
        /// 全日程終了かどうか
        /// </summary>
        public bool IsSessionFinished()
        {
            return CurrentDayIndex >= MaxDays;
        }

        /// <summary>
        /// セッションをリセット（タイトルに戻る時など）
        /// </summary>
        public void ResetSession()
        {
            CurrentDayIndex = 0;
            CurrentFloor = 1;
            DayScores.Clear();
            TotalItems.Clear();
            
            // ショップのコストもリセット
            ShopUI.ResetCost();
            
            Debug.Log("[GameSessionManager] Session Reset");
        }

        /// <summary>
        /// 全ラウンドの合計スコアを取得
        /// </summary>
        public int GetTotalScore()
        {
            int total = 0;
            foreach (var s in DayScores) total += s;
            return total;
        }

        /// <summary>
        /// 特定のラウンド(日)のスコアを取得 (1始まりの日数を指定)
        /// </summary>
        public int GetDayScore(int dayNumber)
        {
            int index = dayNumber - 1;
            if (index >= 0 && index < DayScores.Count)
            {
                return DayScores[index];
            }
            return 0;
        }

        /// <summary>
        /// 指定日のスコアから消費する（ショップ購入用）
        /// </summary>
        public bool SpendFromDayScore(int dayNumber, int amount)
        {
            int index = dayNumber - 1;
            if (index >= 0 && index < DayScores.Count)
            {
                if (DayScores[index] >= amount)
                {
                    DayScores[index] -= amount;
                    Debug.Log($"[GameSessionManager] Spent {amount} from Day {dayNumber}. Remaining: {DayScores[index]}");
                    return true;
                }
                Debug.Log($"[GameSessionManager] Not enough score to spend. Have: {DayScores[index]}, Need: {amount}");
            }
            return false;
        }

        // 互換性維持のためのエイリアス（必要なら）
        public int GetRoundScore(int roundNumber) => GetDayScore(roundNumber);
    }
}
