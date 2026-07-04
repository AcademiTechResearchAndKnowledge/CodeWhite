using UnityEngine;

public class KeyPickup : Interactable
{
    [Header("Inventory Configuration")]
    [SerializeField] private ObjectiveItemData keyItemData;
    [SerializeField] private int amount = 1;

    public override void Interact()
    {
        if (keyItemData == null)
        {
            Debug.LogWarning($"[KeyPickup] No ObjectiveItemData assigned to {gameObject.name}!");
            return;
        }

        // Add the key directly into your objective inventory system
        bool wasPickedUp = ObjectiveInventoryManager.Instance.AddItem(keyItemData, amount);

        if (wasPickedUp)
        {
            Debug.Log($"Player picked up the key: {keyItemData.itemName}!");

            // Optional: If you use an interaction HUD controller like your other pickup scripts:
            // HUDInteractController.Instance.DisableInteractionText();

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Objective Inventory is full! Cannot pick up the key.");
        }
    }
}