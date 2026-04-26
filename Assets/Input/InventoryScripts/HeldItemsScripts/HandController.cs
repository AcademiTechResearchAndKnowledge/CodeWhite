using UnityEngine;

public class HandController : MonoBehaviour
{
    [Header("Randomized Book Models")]
    public GameObject[] handBookModels;

    public void EquipItemByData(ObjectiveItemData itemData)
    {
        UnequipAll();

        if (itemData == null) return;

        // 1. Logic for Randomized Books
        if (itemData.bookType != LibraryBookType.None)
        {
            if (itemData.visualIndex >= 0 && itemData.visualIndex < handBookModels.Length)
            {
                GameObject model = handBookModels[itemData.visualIndex];
                if (model != null)
                {
                    model.SetActive(true);
                    Debug.Log($"[Hand] Activating BOOK model: {model.name} at index {itemData.visualIndex}");
                    return; // Stop here if it's a book
                }
            }
        }

        // 2. Logic for Regular Items (Fallback)
        Transform itemModel = transform.Find(itemData.itemName);
        if (itemModel != null)
        {
            itemModel.gameObject.SetActive(true);
            Debug.Log($"[Hand] Activating REGULAR model by name: {itemModel.name}");
        }
        else
        {
            Debug.LogWarning($"[Hand] Could not find any model for: {itemData.itemName}");
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