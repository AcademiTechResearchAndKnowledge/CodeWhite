using UnityEngine;

public class ContainerHandler : Interactable
{
    public ItemSpawner itemSpawner;
    public bool hasBeenOpened = false;

    public override void Interact()
    {
        Debug.Log("Interact called!");

        if (hasBeenOpened) return;
        hasBeenOpened = true;

        itemSpawner.SpawnRandomItem(transform.position);
    }
}