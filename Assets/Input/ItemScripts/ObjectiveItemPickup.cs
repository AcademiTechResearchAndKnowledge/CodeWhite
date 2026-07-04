using UnityEngine;

public class ObjectiveItemPickup : MonoBehaviour
{
    [Header("The Item Data")]
    public ObjectiveItemData itemData;
    public int amount = 1; // Added amount to give

    public void PickUpItem()
    {
        // Pass the amount to the AddItem method
        bool wasPickedUp = ObjectiveInventoryManager.Instance.AddItem(itemData, amount);

        if (wasPickedUp)
        {
            Debug.Log($"Picked up {amount} {itemData.itemName}");

            HUDInteractController.Instance.DisableInteractionText();
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Objective Inventory is full or cannot carry more of this item!");
        }
    }
}