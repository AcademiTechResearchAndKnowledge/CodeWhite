using UnityEngine;

public class LighterItem : Interactable
{
    public override void Interact()
    {
        Debug.Log("TRACKER: Player clicked the lighter!");

        if (LighterPuzzleManager.instance == null)
        {
            Debug.LogError("CRITICAL ERROR: LighterPuzzleManager is missing from the scene!");
            return;
        }

        LighterPuzzleManager.instance.OnLighterPickedUp();
        Debug.Log("TRACKER: Manager notified. Lighter is picked up.");

        Destroy(gameObject);
    }
}