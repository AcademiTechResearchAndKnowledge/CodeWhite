using UnityEngine;

public class LighterItem : Interactable
{
    [Header("Inventory Data")]
    [Tooltip("Create an ObjectiveItemData for the Lighter and drop it here")]
    public ObjectiveItemData lighterItemData;
    public int amount = 1;

    public override void Interact()
    {
        Debug.Log("TRACKER: Player clicked the lighter!");

        // 1. Safety Checks
        if (LighterPuzzleManager.instance == null)
        {
            Debug.LogError("CRITICAL ERROR: LighterPuzzleManager is missing from the scene!");
            return;
        }

        if (ObjectiveInventoryManager.Instance == null)
        {
            Debug.LogError("CRITICAL ERROR: ObjectiveInventoryManager is missing from the scene!");
            return;
        }

        if (lighterItemData == null)
        {
            Debug.LogError("CRITICAL ERROR: No ObjectiveItemData assigned to the lighter!");
            return;
        }

        // 2. Attempt to add to inventory
        bool wasPickedUp = ObjectiveInventoryManager.Instance.AddItem(lighterItemData, amount);

        // 3. Process successful pickup
        if (wasPickedUp)
        {
            // Notify the puzzle manager
            LighterPuzzleManager.instance.OnLighterPickedUp();
            Debug.Log($"TRACKER: Picked up {amount} {lighterItemData.itemName}. Manager notified.");

            // Clear UI text (Optional but matches your pickup script)
            if (HUDInteractController.Instance != null)
            {
                HUDInteractController.Instance.DisableInteractionText();
            }

            // Destroy the physical lighter
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Objective Inventory is full! Cannot pick up the lighter.");
        }
    }
}