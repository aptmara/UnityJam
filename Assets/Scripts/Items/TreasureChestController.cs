using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Core;  // Inventoryを使うため
using UnityJam.Items; // ItemMasterを使うため
using UnityJam.Interaction;  // 親クラス
using UnityJam.UI;

namespace UnityJam.Gimmicks
{
    public class TreasureChestController : InteractableBase
    {
        [Header("--- ドロップアイテム ---")]
        [Tooltip("このリストの中からランダムで1つ選ばれます")]
        [SerializeField] private List<ItemMaster> dropList;

        [Header("--- 演出 ---")]
        [Tooltip("開いた後の宝箱の見た目（空の箱など）。なければ設定しなくてOK")]
        [SerializeField] private GameObject openedModel;

        [Tooltip("閉まっている状態のモデル（これを開封時に消します）")]
        [SerializeField] private GameObject closedModel;

        [Tooltip("アイテム取得時の飛び出すエフェクト (Prefab)")]
        [SerializeField] private GameObject popupEffectPrefab;

        // 親クラスにある abstract void OnInteractCompleted
        protected override void OnInteractCompleted()
        {
            OpenChest();
        }

        // 宝箱を開ける処理
        void OpenChest()
        {
            // ドロップリストのチェック
            if (dropList == null || dropList.Count == 0)
            {
                Debug.LogWarning("宝箱の中身が空っぽです！InspectorでdropListを設定してください。");
                return;
            }

            // A. ランダム抽選
            int randomIndex = Random.Range(0, dropList.Count);
            ItemMaster item = dropList[randomIndex];

            // B. インベントリに追加
            if (Inventory.Instance != null)
            {
                Inventory.Instance.AddItem(item);
            }

            // C. 見た目の切り替え（閉じた箱を消して、開いた箱を表示）
            if (closedModel != null) closedModel.SetActive(false);
            if (openedModel != null) openedModel.SetActive(true);

            // D. アイテム飛び出し演出
            if (popupEffectPrefab != null)
            {
                // 1. 生成して、そのGameObjectを変数に入れる
                GameObject effectObj = Instantiate(popupEffectPrefab, transform.position + Vector3.up, Quaternion.identity);

                // 2. そのオブジェクトについている ItemPopupEffect スクリプトを取得する
                ItemPopupEffect popupScript = effectObj.GetComponent<ItemPopupEffect>();

                // 3. スクリプトがあれば、アイコン画像を渡して初期化する
                if (popupScript != null)
                {
                    popupScript.Initialize(item.icon);
                }
                else
                {
                    Debug.LogWarning("PopupPrefabに 'ItemPopupEffect' スクリプトがついていません！");
                }
            }

            Debug.Log($"宝箱を開けた！ {item.itemName} を獲得！");
        }
    }
}
