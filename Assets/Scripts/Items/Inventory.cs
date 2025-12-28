using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Items;

namespace UnityJam.Core
{
    public class Inventory : MonoBehaviour
    {
        // シングルトン（どこからでもアクセス可能にする）
        public static Inventory Instance { get; private set; }
        // アイテムの保管場所（辞書形式: データ, 個数）
        private readonly Dictionary<ItemMaster, int> _items = new();

        // 変更通知イベント
        public event Action OnInventoryUnlocked;                    // 初めて拾った時
        public event Action<ItemMaster, int> OnItemCountChanged;    // 数が変わった時
        // 重量が変化した時のイベント（UI更新用）
        public event Action<float> OnWeightChanged;
        public event Action<float> OnConsumptionRateChanged;

        // 状態：インベントリが解放されているか（最初のアイテム取得でtrue）
        public bool isUnlocked { get; private set; } = false;

        // 総重量プロパティ
        public float TotalWeight { get; private set; } = 0f;

        // 総スコア（リザルト用）
        public int TotalScore { get; private set; } = 0;

        // 総稼働率
        public float TotalConsumptionRate { get; private set; } = 0f;

        // 重量によるペナルティを有効にするかどうかのフラグ
        [Header("Settings")]
        [Tooltip("重量による移動速度低下を有効にするか")]
        public bool enableWeightPenalty = false;

        // シングルトンの初期化
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        // アイテムを追加する
        public void AddItem(ItemMaster item, int count = 1)
        {
            // 初回取得ならアンロック通知
            if(!isUnlocked)
            {
                isUnlocked = true;
                OnInventoryUnlocked?.Invoke();
            }

            if (_items.ContainsKey(item))
                _items[item] += count;
            else
                _items[item] = count;

            // 重量 & 総スコア 再計算
            CalculateTotals();

            Debug.Log($"[Inventory] Get: {item.itemName} (Total: {_items[item]})");
            OnItemCountChanged?.Invoke(item, _items[item]);
        }

        // アイテムを捨てる（削除する）処理
        public void RemoveItem(ItemMaster item, int count = 1)
        {
            if (!_items.ContainsKey(item)) return;

            _items[item] -= count;
            int remaining = _items[item];

            if (remaining <= 0)
            {
                _items.Remove(item);
                remaining = 0;
            }

            // 重量 & 総スコア 再計算
            CalculateTotals();

            Debug.Log($"[Inventory] Discard: {item.itemName} (Remaining: {remaining})");
            OnItemCountChanged?.Invoke(item, remaining);
        }

        // 消費したスコア
        private int _spentScore = 0;

        // 重量とスコアをまとめて計算する
        private void CalculateTotals()
        {
            float totalW = 0f;
            int totalS = 0;
            float totalC = 0f; // 稼働率計算用

            foreach (var kvp in _items)
            {
                ItemMaster item = kvp.Key;
                int count = kvp.Value;

                // 重さ (単体重さ * 個数)
                totalW += item.weight * count;

                // スコア (単体価値 * 個数)
                totalS += item.value * count;

                // 稼働率 (単体稼働率 * 個数)
                totalC += item.consumptionRate * count;
            }

            // 結果を保存
            TotalWeight = totalW;
            // 獲得スコア - 消費スコア (0未満にはならないようにする)
            TotalScore = Mathf.Max(0, totalS - _spentScore);
            TotalConsumptionRate = totalC;

            // イベント通知
            OnWeightChanged?.Invoke(TotalWeight);

            // 稼働率変更通知
            OnConsumptionRateChanged?.Invoke(TotalConsumptionRate);
        }

        /// <summary>
        /// スコアを消費する（ショップなどの支払いで使用）
        /// </summary>
        public void SpendScore(int amount)
        {
            if (amount < 0) return;
            _spentScore += amount;
            CalculateTotals();
            Debug.Log($"[Inventory] Spent Score: {amount} (Total Spent: {_spentScore})");
        }

        // 全アイテムを取得（表示用）
        public Dictionary<ItemMaster, int> GetAllItems() => _items;

        // インベントリを全て空にする
        public void Clear()
        {
            foreach (var kvp in _items)
            {
                OnItemCountChanged?.Invoke(kvp.Key, 0);
            }

            _items.Clear();
            _spentScore = 0; // 消費スコアもリセット
            TotalWeight = 0f;
            TotalScore = 0;
            TotalConsumptionRate = 0f;

            OnWeightChanged?.Invoke(TotalWeight);
            OnConsumptionRateChanged?.Invoke(TotalConsumptionRate);
        }
    }

}
