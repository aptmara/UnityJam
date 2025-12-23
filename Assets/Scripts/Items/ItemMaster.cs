using Unity.VisualScripting;
using UnityEngine;

namespace UnityJam.Items
{
    /// <summary>
    /// アイテム1つ1つのデータを定義する設計図（ScriptableObject）
    /// Projectウィンドウで右クリック > UnityJam > Item Masterで作成
    /// </summary>
    [CreateAssetMenu(fileName = "Item_NewTreasure", menuName = "UnityJam/Item Master")]
    public class ItemMaster : ScriptableObject
    {
        // 基本情報
        // ============================================================
        [Header("--- 基本データ ---")]
        [Tooltip("ゲーム内で表示される名前")]
        public string itemName = "新しいお宝";

        [Tooltip("インベントリで表示するアイコン")]
        public Sprite icon;

        [Tooltip("アイテムの説明文")]
        [TextArea(3, 5)] public string description = "ここに説明文を書く";

        // パラメータ
        // ============================================================
        [Header("--- パラメータ ---")]
        [Tooltip("価値（スコアや通貨換算用）")]
        [Min(0)] public int value = 100;

        [Tooltip("重さ（持ち運び制限用など）")]
        [Min(0)] public float weight = 1.0f;

        [Tooltip("レアリティ（1〜3の星の数）")]
        [Range(1, 3)] public int rarity = 1;

        /// <summary>
        /// 便利機能：インスペクターで値を変更した時、
        /// アイテム名が空ならファイル名を自動で入れる。
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemName))
                itemName = name;
        }
    }
}
