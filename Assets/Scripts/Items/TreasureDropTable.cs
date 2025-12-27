using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Core;
using UnityJam.Items;

namespace UnityJam.Core
{
    [System.Serializable]
    public class DropEntry
    {
        [Tooltip("ドロップするアイテム")]
        public ItemMaster item;

        [Tooltip("確率の重み（バーで操作する値）")]
        [Min(1)] public int weight = 10;
    }

    [CreateAssetMenu(fileName = "NewDropTable", menuName = "UnityJam/Treasure Drop Table")]
    public class TreasureDropTable : ScriptableObject
    {
        [Header("--- Trap Settings ---")]
        [Tooltip("宝箱を開けた時に減少するバッテリー量（最大値の何%を減らすか）\n例: 10 なら 10% 減る")]
        [Range(0, 100)] public float batteryPenaltyPercent = 10.0f;

        [Header("--- Drop List ---")]
        [Tooltip("ドロップリスト（下のグラフで調整してください）")]
        public List<DropEntry> dropList = new List<DropEntry>();

        /// <summary>
        /// 抽選ロジック
        /// </summary>
        public ItemMaster PickOneItem()
        {
            if (dropList == null || dropList.Count == 0) return null;

            // 1. 総重量を計算
            int totalWeight = 0;
            foreach (var entry in dropList)
            {
                if (entry.item != null) totalWeight += entry.weight;
            }

            if (totalWeight == 0) return null;

            // 2. ランダム抽選
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;

            foreach (var entry in dropList)
            {
                if (entry.item == null) continue;

                currentWeight += entry.weight;
                if (randomValue < currentWeight)
                {
                    return entry.item;
                }
            }
            return null;
        }
    }
}
