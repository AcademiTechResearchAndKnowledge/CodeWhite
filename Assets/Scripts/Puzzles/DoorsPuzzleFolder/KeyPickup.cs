using UnityEngine;

public class KeyPickup : Interactable
{
    public override void Interact()
    {
        DoorPuzzleHandler.instance.hasKey = true;

        Debug.Log("Player picked up the key!");

        Destroy(gameObject);
    }
}