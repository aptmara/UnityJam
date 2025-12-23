using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityJam.Core;
using UnityJam.Items;
using TMPro;
using System.Linq; // リスト操作用

namespace UnityJam.UI
{
    public class InventoryView : MonoBehaviour
    {
        [Header("--- Input Settings ---")]
        [Tooltip("インベントリを開閉するキー")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Tab;

        [Header("--- Player Control ---")]
        [Tooltip("インベントリ中に停止させたいスクリプト（移動、カメラ、インタラクト等）をここに登録")]
        [SerializeField] private List<MonoBehaviour> scriptsToDisable;

        [Header("--- UI References ---")]
        [Tooltip("全体のパネル（表示/非表示用）")]
        [SerializeField] private GameObject inventoryPanel;

        [Tooltip("スロットを並べる親オブジェクト")]
        [SerializeField] private Transform slotGridParent;

        [Tooltip("スロットのプレファブ")]
        [SerializeField] private InventorySlot slotPrefab;

        [Header("--- Description Area ---")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Image descriptionIcon;

        private List<InventorySlot> spawnedSlots = new List<InventorySlot>();
        private const int MAX_SLOTS = 15;

        // 初期化フラグ
        private bool isInitialized = false;

        void Start()
        {
            // パネルを初期化（最初は閉じておく）
            inventoryPanel.SetActive(false);

            // スロット生成
            InitializeSlots();

            // イベント購読（シングルトンがあれば）
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnInventoryUnlocked += ForceOpenInventory; // 初回アンロック時は強制的に開く
                Inventory.Instance.OnItemCountChanged += RefreshInventory;
            }
        }

        // キー入力を監視する
        void Update()
        {
            // インベントリ機能自体がアンロックされていない場合は開かない（必要ならこのifを外してください）
            //if (Inventory.Instance != null && !Inventory.Instance.isUnlocked) return;

            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInventory();
            }
        }

        // 開閉を切り替える
        public void ToggleInventory()
        {
            bool isActive = !inventoryPanel.activeSelf;

            if (isActive)
            {
                OpenInventory();
            }
            else
            {
                CloseInventory();
            }
        }

        // 開く処理
        void OpenInventory()
        {
            inventoryPanel.SetActive(true);
            RefreshInventory(null, 0); // データ更新

            // カーソルを表示して操作できるようにする
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SetPlayerScriptsActive(false);
        }

        // イベント用（強制オープン）
        void ForceOpenInventory()
        {
            OpenInventory();
        }

        // 登録されたスクリプトを一括でON/OFFする関数
        void SetPlayerScriptsActive(bool isActive)
        {
            if (scriptsToDisable != null)
            {
                foreach (var script in scriptsToDisable)
                {
                    if (script != null) script.enabled = isActive;
                }
            }

            // デバッグ用ログ（Consoleに出ます）
            Debug.Log($"プレイヤー操作を {(isActive ? "再開" : "停止")} しました");
        }

        // 閉じる処理
        void CloseInventory()
        {
            inventoryPanel.SetActive(false);

            // カーソルを消してゲームに戻る（FPS/TPSの場合）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            SetPlayerScriptsActive(true);
        }

        void InitializeSlots()
        {
            foreach (Transform child in slotGridParent) Destroy(child.gameObject);
            spawnedSlots.Clear();

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                InventorySlot newSlot = Instantiate(slotPrefab, slotGridParent);
                newSlot.ClearSlot();
                newSlot.OnSlotClicked += HandleSlotSelected;
                spawnedSlots.Add(newSlot);
            }
        }

        // データ更新（開いた時やアイテム増減時に呼ばれる）
        void RefreshInventory(ItemMaster item, int count)
        {
            // パネルが閉じてるなら描画更新しない（負荷軽減）
            if (!inventoryPanel.activeSelf) return;
            if (Inventory.Instance == null) return;

            var allItems = Inventory.Instance.GetAllItems();
            var itemList = allItems.Keys.ToList();

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (i < itemList.Count)
                {
                    ItemMaster data = itemList[i];
                    int amount = allItems[data];
                    spawnedSlots[i].SetItem(data, amount);
                }
                else
                {
                    spawnedSlots[i].ClearSlot();
                }
            }
        }

        void HandleSlotSelected(InventorySlot slot)
        {
            foreach (var s in spawnedSlots) s.SetSelected(false);
            slot.SetSelected(true);
            UpdateDescription(slot.ItemData);
        }

        void UpdateDescription(ItemMaster item)
        {
            if (item == null)
            {
                nameText.text = "";
                descriptionText.text = "";
                descriptionIcon.enabled = false;
                return;
            }

            nameText.text = item.itemName;
            descriptionText.text = item.description;
            descriptionIcon.sprite = item.icon;
            descriptionIcon.enabled = true;
        }
    }
}
