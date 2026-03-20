using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public ItemData itemData;
    public int amount = 1;

    public void Pickup()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("No InventoryManager found.");
            return;
        }

        if (itemData == null)
        {
            Debug.LogWarning("No ItemData assigned to pickup.");
            return;
        }

        bool added = InventoryManager.Instance.AddItem(itemData, amount);

        if (added)
        {
            Debug.Log($"Picked up {amount} {itemData.itemName}");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"{itemData.itemName} is full.");
        }
    }
}