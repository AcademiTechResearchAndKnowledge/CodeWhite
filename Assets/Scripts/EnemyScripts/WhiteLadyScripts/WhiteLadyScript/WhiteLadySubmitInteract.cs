using UnityEngine;

public class WhiteLadySubmitInteract : Interactable
{
    private WhiteLady whiteLady;

    [Header("Accepted Submission Items")]
    public ObjectiveItemData flowerData;
    public ObjectiveItemData fixedMirrorData;

    void Awake()
    {
        whiteLady = GetComponentInParent<WhiteLady>();
        DisableOutline();
    }

    public override void Interact()
    {
        if (whiteLady == null) return;

        if (whiteLady.CurrentState != WhiteLady.State.Weeping)
        {
            Debug.Log("She is too hostile right now! You can only approach her when she is weeping.");
            return;
        }

        // Grab whatever the player is currently holding
        ObjectiveInventorySlot activeSlot = ObjectiveInventoryManager.Instance.GetSelectedSlot();

        if (activeSlot == null || activeSlot.IsEmpty())
        {
            Debug.Log("You need to hold the item in your hand to submit it!");
            return;
        }

        ObjectiveItemData heldItem = activeSlot.item;

        // Verify the held item is one of the accepted items
        if (heldItem == flowerData || heldItem == fixedMirrorData)
        {
            base.Interact();
            Debug.Log($"Successfully submitted {heldItem.itemName} to the White Lady!");

            // ---------------------------------------------------------
            // NEW: DETERMINE THE "OTHER" ITEM TO DESTROY
            // ---------------------------------------------------------
            // If we are holding the flower, the other item is the mirror. If holding the mirror, it's the flower.
            ObjectiveItemData otherItem = (heldItem == flowerData) ? fixedMirrorData : flowerData;

            // 1. Consume the item currently in our hand
            activeSlot.amount--;
            if (activeSlot.amount <= 0)
            {
                activeSlot.Clear();
                ObjectiveInventoryManager.Instance.DeselectAll(); // Drop hand visual model
            }

            // 2. NEW: Search the entire inventory and erase the other item if it exists
            ObjectiveInventoryManager.Instance.RemoveItem(otherItem, 1);

            // 3. Refresh the UI once so the player sees both changes simultaneously
            ObjectiveInventoryManager.Instance.RefreshUI();
            // ---------------------------------------------------------

            // 4. Subdue the entity
            whiteLady.gameObject.SetActive(false);

            // 5. Unlock progression and spawn the portal
            WLObjectiveManager.Instance.UnlockProgress($"Submitted {heldItem.itemName} to White Lady");

            // 6. Destroy the hitbox
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("The White Lady does not want this item.");
        }
    }
}