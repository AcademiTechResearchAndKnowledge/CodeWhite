using UnityEngine;

public class WoodItem : MonoBehaviour
{
    public string itemId = "Wood";
    public int amount = 1;

    public void Pickup()
    {
        // TODO: add to inventory here
        Debug.Log($"Picked up {amount} {itemId}");

        // remove object from the world
        Destroy(gameObject);
    }
}