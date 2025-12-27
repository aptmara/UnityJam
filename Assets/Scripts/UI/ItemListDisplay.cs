using System.Collections.Generic;
using UnityEngine;
using UnityJam.Core;
using UnityJam.Items;

namespace UnityJam.UI
{
    public class ItemListDisplay : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject itemSlotPrefab;

        [SerializeField] private bool autoBindInventory = true; // Inventoryを自動で監視するか

        private Dictionary<ItemMaster, ItemSlotUI> _currentSlots = new();

        private void Start()
        {
            if (autoBindInventory && Inventory.Instance != null)
            {
                // イベント購読
                Inventory.Instance.OnItemCountChanged += OnItemCountChanged;

                // 初期表示
                RefreshAll(Inventory.Instance.GetAllItems());
            }
        }

        private void OnDestroy()
        {
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnItemCountChanged -= OnItemCountChanged;
            }
        }

        /// <summary>
        /// 外部からアイテムリストを指定して表示する（リザルト画面用など）
        /// </summary>
        public void ShowItems(Dictionary<ItemMaster, int> items)
        {
            // 自動監視を切る（念のため）
            autoBindInventory = false; 
            RefreshAll(items);
        }

        private void RefreshAll(Dictionary<ItemMaster, int> items)
        {
            // 一旦クリア（必要なら最適化できるが、アイテム数はそこまで多くないと仮定）
            foreach (var slot in _currentSlots.Values)
            {
                Destroy(slot.gameObject);
            }
            _currentSlots.Clear();

            foreach (var kvp in items)
            {
                CreateOrUpdateSlot(kvp.Key, kvp.Value);
            }
        }

        private void OnItemCountChanged(ItemMaster item, int count)
        {
            CreateOrUpdateSlot(item, count);
        }

        private void CreateOrUpdateSlot(ItemMaster item, int count)
        {
            // 個数が0以下なら削除
            if (count <= 0)
            {
                if (_currentSlots.ContainsKey(item))
                {
                    Destroy(_currentSlots[item].gameObject);
                    _currentSlots.Remove(item);
                }
                return;
            }

            // 既存スロットの更新または新規作成
            if (_currentSlots.ContainsKey(item))
            {
                _currentSlots[item].Setup(item, count);
            }
            else
            {
                if (itemSlotPrefab != null && listContainer != null)
                {
                    GameObject obj = Instantiate(itemSlotPrefab, listContainer);
                    ItemSlotUI slotUI = obj.GetComponent<ItemSlotUI>();
                    if (slotUI != null)
                    {
                        slotUI.Setup(item, count);
                        _currentSlots.Add(item, slotUI);
                    }
                }
            }
        }
    }
}
