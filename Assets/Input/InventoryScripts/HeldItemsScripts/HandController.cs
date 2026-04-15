using UnityEngine;

public class HandController : MonoBehaviour
{
    public void EquipItemByName(string itemName)
    {
        // First, hide every item in the hand
        UnequipAll();

        if (string.IsNullOrEmpty(itemName)) return;

        // Search the children of the Hand for one with the exact matching name
        Transform itemModel = transform.Find(itemName);

        if (itemModel != null)
        {
            itemModel.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"HandController: No 3D model found in Hand named '{itemName}'. Make sure the GameObject name matches the ScriptableObject itemName exactly!");
        }
    }

    public void UnequipAll()
    {
        // Loop through all children and disable them
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}