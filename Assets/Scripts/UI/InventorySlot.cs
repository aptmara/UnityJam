using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // クリック検知用
using System;
using UnityJam.Items;

namespace UnityJam.UI
{
    // 1つのアイテム枠を管理するクラス
    public class InventorySlot : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI Components")]
        [SerializeField] private Image iconImage;          // アイコン画像
        [SerializeField] private TextMeshProUGUI amountText; // 個数テキスト
        [SerializeField] private Image selectionFrame;     // 選択時の黄色い枠
        [SerializeField] private Image lockIcon;           // ロック時の鍵アイコン

        // クリックされた時にViewに通知するためのイベント
        public event Action<InventorySlot> OnSlotClicked;

        // このスロットに入っているアイテムデータ
        public ItemMaster ItemData { get; private set; }

        // 初期化（空の状態にする）
        public void ClearSlot()
        {
            ItemData = null;
            iconImage.sprite = null;
            iconImage.enabled = false;     // アイコンを隠す
            amountText.text = "";
            selectionFrame.enabled = false;
            lockIcon.enabled = false;

            // ボタンとしての機能を無効化（空っぽならクリックさせない場合）
            // GetComponent<Button>().interactable = false; 
        }

        // アイテムをセットする
        public void SetItem(ItemMaster item, int count)
        {
            ItemData = item;

            // アイコン表示
            iconImage.sprite = item.icon;
            iconImage.enabled = true;

            // 個数表示（1個なら表示しない、などの調整も可）
            amountText.text = count > 1 ? count.ToString() : "";

            lockIcon.enabled = false;
        }

        // 選択状態の切り替え（黄色い枠のON/OFF）
        public void SetSelected(bool isSelected)
        {
            if (selectionFrame != null)
                selectionFrame.enabled = isSelected;
        }

        // クリックされた時の処理（Unity標準のインターフェース）
        public void OnPointerClick(PointerEventData eventData)
        {
            // 中身がある時だけクリック反応
            if (ItemData != null)
            {
                OnSlotClicked?.Invoke(this);
            }
        }
    }
}
