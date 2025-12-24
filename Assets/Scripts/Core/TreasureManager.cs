using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityJam.Core
{
    /// <summary>
    /// シーン内の宝箱の残数を管理するクラス
    /// </summary>
    public class TreasureManager : MonoBehaviour
    {
        public static TreasureManager Instance { get; private set; }

        // 現在シーンにある未開封の宝箱リスト
        private List<GameObject> activeChests = new List<GameObject>();

        // 宝箱の数が変わった時のイベント
        public event Action OnTreasureCountChanged;

        private void Awake()
        {
            // シーン単位で管理したいので、シーン遷移で破壊されて新しく作り直される通常のSingletonにする
            if(Instance != null & Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// 宝箱が生成された時（Start）に呼ばれる
        /// </summary>
        public void RegisterChest(GameObject chest)
        {
            if (!activeChests.Contains(chest))
            {
                activeChests.Add(chest);
                //OnTreasureCountChanged?.Invoke();
            }
        }

        /// <summary>
        /// 宝箱が開けられた時に呼ばれる
        /// </summary>
        public void UnregisterChest(GameObject chest)
        {
            if (activeChests.Contains(chest))
            {
                activeChests.Remove(chest);
                OnTreasureCountChanged?.Invoke();
            }
        }

        /// <summary>
        /// UI表示用の漠然としたヒントを返す
        /// </summary>
        public string GetTreasureHint()
        {
            int count = activeChests.Count;

            if (count == 0) return "このあたりの気配は消えたようだ...";
            if (count == 1) return "あと少し、お宝の気配がする...";
            if (count < 5)  return "まだどこかにお宝があるはずだ。";

            return "強いお宝の気配を感じる！";
        }
    }
}
