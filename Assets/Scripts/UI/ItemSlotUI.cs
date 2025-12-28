using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityJam.Items;

namespace UnityJam.UI
{
    public class ItemSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text countLabel;

        public void Setup(ItemMaster item, int count)
        {
            if (item == null) return;

            if (iconImage != null)
            {
                iconImage.sprite = item.icon;
                // アイコンがない場合は透明にするなどの処理が必要ならここに追加
                iconImage.enabled = item.icon != null; 
            }

            if (nameLabel != null)
            {
                nameLabel.text = item.itemName;
            }

            if (countLabel != null)
            {
                // 個数が1個なら表示しない、などの仕様も考えられるが、一旦そのまま表示
                countLabel.text = $"x{count:N0}";
            }
        }
    }
}
