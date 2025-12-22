using UnityEngine;

public enum ItemType
{
    Treasure,
    Consumable,
    KeyItem,
}

public enum OnPickupEffect
{
    None,
    AccelerateLightDecay, // 誘惑宝の効果
}

[CreateAssetMenu(fileName = "Item_", menuName = "Game/Item Master")]
public class ItemMaster : ScriptableObject
{
    [Header("Identity")]
    [Min(0)] public int itemId;          // 12, 13 ...
    public string itemName;             // "通常宝" など
    public ItemType itemType = ItemType.Treasure;

    [Header("UI")]
    public Sprite icon;
    [TextArea] public string description;

    [Header("Pickup")]
    public OnPickupEffect onPickupEffect = OnPickupEffect.None;

    // 便利：インスペクタで名前が分かりやすくなる
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemName))
            itemName = name;
    }
}
