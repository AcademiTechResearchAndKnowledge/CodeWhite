using UnityEngine;

public class LighterItem : Interactable
{
    public static bool hasLighter = false;

    public override void Interact()
    {
        hasLighter = true;
        Destroy(gameObject); // Destroy prefab after pickup
        Debug.Log("Picked up lighter!");
    }
}