using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityJam.Interaction;

namespace UnityJam.Gimmicks
{

    public class MimicChestController : InteractableBase
    {
        [Header("--- 演出 ---")]
        [Tooltip("正体を現した時のモデル（モンスターの姿など）")]
        [SerializeField] private GameObject monsterModel;

        [Tooltip("擬態中のモデル（普通の宝箱）")]
        [SerializeField] private GameObject boxModel;

        protected override void OnInteractCompleted()
        {
            ActivateTrap();
        }

        void ActivateTrap()
        {
            Debug.Log("<color=red>ミミック覚醒！！うぎゃぁぁぁっ！！ぶち56された！！GAME OVER</color>");

            // 1. 姿を変える
            if (boxModel != null) boxModel.SetActive(false);
            if (monsterModel != null) monsterModel.SetActive(true);

            // 2. プレイヤーを即死させる
            // プレイヤーのオブジェクトを破壊
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Destroy(player);
            }
        }
    }
}
