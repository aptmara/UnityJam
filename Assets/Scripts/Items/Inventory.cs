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

        // 状態：インベントリが解放されているか（最初のアイテム取得でtrue）
        public bool isUnlocked { get; private set; } = false;

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
                OnInventoryUnlocked.Invoke();
            }

            if (_items.ContainsKey(item))
                _items[item] += count;
            else
                _items[item] = count;

            Debug.Log($"[Inventory] Get: {item.itemName} (Total: {_items[item]})");
            OnItemCountChanged?.Invoke(item, _items[item]);
        }

        // 全アイテムを取得（表示用）
        public Dictionary<ItemMaster, int> GetAllItems() => _items;
    }

}
