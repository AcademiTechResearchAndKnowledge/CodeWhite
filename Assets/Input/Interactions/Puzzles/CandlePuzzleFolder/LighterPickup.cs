using UnityEngine;

public class LighterPickup : Interactable
{
    public static bool hasLighter = false;

    public override void Interact()
    {
        hasLighter = true;
        Destroy(gameObject); // Destroy prefab after pickup
        Debug.Log("Picked up lighter!");
    }
}