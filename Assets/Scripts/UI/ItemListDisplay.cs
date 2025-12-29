using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For Layout Components
using UnityJam.Core;
using UnityJam.Items;

namespace UnityJam.UI
{
    public class ItemListDisplay : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform listContainer;
        [SerializeField] private GameObject itemSlotPrefab;

        [Header("Layout Settings")]
        [SerializeField] private bool useAutoLayout = true;
        [SerializeField] private Vector2 cellSize = new Vector2(100, 100);
        [SerializeField] private Vector2 spacing = new Vector2(10, 10);
        [SerializeField] private GridLayoutGroup.Constraint constraint = GridLayoutGroup.Constraint.Flexible;
        [SerializeField] private int constraintCount = 0;

        [SerializeField] private bool autoBindInventory = true; // Inventoryを自動で監視するか

        private Dictionary<ItemMaster, ItemSlotUI> _currentSlots = new();

        private void Start()
        {
            if (useAutoLayout && listContainer != null)
            {
                SetupLayout();
            }

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

        private void SetupLayout()
        {
            // GridLayoutGroupの取得または追加
            GridLayoutGroup grid = listContainer.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = listContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            grid.cellSize = cellSize;
            grid.spacing = spacing;
            grid.constraint = constraint;
            grid.constraintCount = constraintCount;
            // 必要に応じてAlignmentなども設定
            grid.childAlignment = TextAnchor.UpperLeft; 

            // ContentSizeFitterの取得または追加
            ContentSizeFitter fitter = listContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = listContainer.gameObject.AddComponent<ContentSizeFitter>();
            }

            // 縦方向に伸びる設定（スクロールビューの中身想定）
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>
        /// 外部からアイテムリストを指定して表示する（リザルト画面用など）
        /// </summary>
        public void ShowItems(Dictionary<ItemMaster, int> items)
        {
            // 自動監視を切る（念のため）
            autoBindInventory = false; 
            
            // レイアウト設定（Startで呼ばれてない場合のため）
            if (useAutoLayout && listContainer != null)
            {
                SetupLayout();
            }

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
