using System.Collections.Generic;
using UnityEngine;
using UnityJam.Core;         // Inventoryを使うため
using UnityJam.Items;        // ItemMasterを使うため
using UnityJam.Interaction;  // 親クラス
using UnityJam.UI;
using UnityJam.Effects;      // BloomBurst注入用

namespace UnityJam.Gimmicks
{
    public class TreasureChestController : InteractableBase, IBloomBurstReceiver
    {
        [Header("--- ドロップアイテム ---")]
        [Tooltip("このリストの中からランダムで1つ選ばれます")]
        [SerializeField] private List<ItemMaster> dropList;

        [Header("--- 演出 ---")]
        [Tooltip("開いた後の宝箱の見た目（空の箱など）。なければ設定しなくてOK")]
        [SerializeField] private GameObject openedModel;

        [Tooltip("閉まっている状態のモデル（これを開封時に消します）")]
        [SerializeField] private GameObject closedModel;

        [Header("--- エフェクト設定 ---")]
        [Tooltip("宝箱を開けた瞬間のエフェクト（VFX）")]
        [SerializeField] private GameObject openVfxPrefab;

        [Tooltip("アイテム取得時の飛び出すエフェクト (Prefab)")]
        [SerializeField] private GameObject popupEffectPrefab;

        [Header("--- ポストエフェクト ---")]
        [Tooltip("開封時にBloomを一瞬だけ強める（Spawnerから自動注入される想定）")]
        [SerializeField] private BloomBurstController bloomBurst;

        [Header("--- Light Burst (Optional) ---")]
        [Tooltip("宝箱開封時に一瞬だけ出す Point Light のPrefab（PFx_TreasureBurstLight など）")]
        [SerializeField] private GameObject burstLightPrefab;

        [SerializeField, Min(0.01f)]
        private float burstLightLifeTime = 0.35f;

        [SerializeField]
        private Vector3 burstLightOffset = new Vector3(0f, 1.0f, 0f);

        /// <summary>
        /// TreasureSpawner 等から BloomBurstController を注入する。
        /// </summary>
        public void SetBloomBurst(BloomBurstController bloomBurstController)
        {
            bloomBurst = bloomBurstController;
        }

        // 親クラスにある abstract void OnInteractCompleted
        protected override void OnInteractCompleted()
        {
            OpenChest();
        }

        // 宝箱を開ける処理
        private void OpenChest()
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

            // D. エフェクトの再生
            if (openVfxPrefab != null)
            {
                Instantiate(openVfxPrefab, transform.position, Quaternion.identity);
            }

            // D-1. Light Burst（確実に「光った」感を出す）
            if (burstLightPrefab != null)
            {
                GameObject lightObj = Instantiate(
                    burstLightPrefab,
                    transform.position + burstLightOffset,
                    Quaternion.identity);

                Destroy(lightObj, burstLightLifeTime);
            }

            // D-2. Bloom ブースト（パターンA：Global VolumeのBloomを一瞬だけ強める）
            if (bloomBurst != null)
            {
                bloomBurst.PlayBurst();
            }

            // E. アイテム飛び出し演出
            if (popupEffectPrefab != null)
            {
                GameObject effectObj = Instantiate(popupEffectPrefab, transform.position + Vector3.up, Quaternion.identity);

                ItemPopupEffect popupScript = effectObj.GetComponent<ItemPopupEffect>();
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
