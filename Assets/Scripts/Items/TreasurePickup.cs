using UnityEngine;

public class TreasurePickup : MonoBehaviour
{
    [SerializeField] private ItemMaster item;
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Inventory.Instance.AddItem(item, amount);

        // 誘惑宝の効果
        if (item.onPickupEffect == OnPickupEffect.AccelerateLightDecay)
        {

        }

        Destroy(gameObject);
    }
}
