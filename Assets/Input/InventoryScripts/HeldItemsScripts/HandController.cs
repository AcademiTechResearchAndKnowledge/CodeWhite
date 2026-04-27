using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Randomized Book Models")]
    public GameObject[] handBookModels;

    // --- Method 1: For Objective Items (Books AND Normal Objective Items) ---
    public void EquipItemByData(ObjectiveItemData itemData)
    {
        UnequipAll();

        if (itemData == null) return;

        // 1. Is it a Randomized Book?
        if (itemData.bookType != LibraryBookType.None)
        {
            if (itemData.visualIndex >= 0 && itemData.visualIndex < handBookModels.Length)
            {
                GameObject model = handBookModels[itemData.visualIndex];
                if (model != null)
                {
                    model.SetActive(true);
                    Debug.Log($"[Hand] Activating BOOK model: {model.name} at index {itemData.visualIndex}");

                    // STOP HERE if we successfully equipped the book
                    return;
                }
            }
        }

        // 2. FALLBACK: If it's an Objective Item, but NOT a book, just use its name!
        EquipItemByName(itemData.itemName);
    }

    // --- Method 2: For Regular Items (ItemData) AND our Fallback ---
    public void EquipItemByName(string itemName)
    {
        UnequipAll(); // Just in case, clear again

        if (string.IsNullOrEmpty(itemName)) return;

        // Search the children of the Hand for one with the exact matching name
        Transform itemModel = transform.Find(itemName);

        if (itemModel != null)
        {
            itemModel.gameObject.SetActive(true);
            Debug.Log($"[Hand] Activating model by name: {itemModel.name}");
        }
        else
        {
            Debug.LogWarning($"[Hand] Could not find any model for: {itemName}. Make sure the child object's name matches exactly!");
        }
    }

    public void UnequipAll()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}