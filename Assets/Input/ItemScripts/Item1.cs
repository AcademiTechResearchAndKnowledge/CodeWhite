using UnityEngine;

public class Item1 : MonoBehaviour
{
    [Header("Item")]
    public string itemId = "KeyItem";
    public int amount = 1;

    [Header("Portal")]
    [SerializeField] private RandomPortalSpawner portalSpawner;

    public void Pickup()
    {
        // spawn portal
        if (portalSpawner != null)
            portalSpawner.SpawnPortalRandom();
        else
            Debug.LogWarning("[KeyItemPickup] portalSpawner not assigned."); //for debugging

        // remove item
        Destroy(gameObject);
    }
}