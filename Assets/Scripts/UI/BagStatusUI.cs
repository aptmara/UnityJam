using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityJam.Core;

namespace UnityJam.UI
{
    public class BagStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("変化させるカバンのImage")]
        [SerializeField] private Image bagImage;

        [Header("Settings")]
        [Tooltip("カバンの画像リスト（0:空っぽ 〜 最後:パンパン）の順で登録")]
        [SerializeField] private Sprite[] bagSprites;

        [Tooltip("この重量以上で「パンパン（最後の画像）」になる")]
        [SerializeField] private float maxWeightCapacity = 20.0f;

        void Start()
        {
            if (Inventory.Instance != null)
            {
                // 重量が変更されたら更新するイベントを購読
                Inventory.Instance.OnWeightChanged += UpdateBagImage;

                // 初回の表示更新
                UpdateBagImage(Inventory.Instance.TotalWeight);
            }
        }

        void OnDestroy()
        {
            // イベント購読の解除（エラー防止）
            if (Inventory.Instance != null)
            {
                Inventory.Instance.OnWeightChanged -= UpdateBagImage;
            }
        }

        // 重量を受け取って画像を切り替える
        void UpdateBagImage(float currentWeight)
        {
            if (bagSprites == null || bagSprites.Length == 0) return;

            // 0 〜 1 の割合（パーセント）を計算
            float percentage = Mathf.Clamp01(currentWeight / maxWeightCapacity);

            // 割合に応じて配列のインデックスを決定
            // 例: 画像が3枚なら、0〜0.33 -> 0番目, 0.34〜0.66 -> 1番目, 0.67〜1.0 -> 2番目
            int index = Mathf.FloorToInt(percentage * (bagSprites.Length - 1));

            // 画像を変更
            if (bagImage != null)
            {
                bagImage.sprite = bagSprites[index];
            }
        }
    }
}
