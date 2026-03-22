using UnityEngine;

public class Point1 : MonoBehaviour
{
    [Header("Item")]
    public string itemId = "KeyItem";
    public int amount = 1;

    public void Pickup()
    {
        // Tell PointManager that a point was collected
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoint();
        }
        else
        {
            Debug.LogWarning("No PointManager in scene!");
        }

        Debug.Log($"Picked up {amount} {itemId}");

        // Destroy immediate parent (if it exists)
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }

        Destroy(gameObject);
    }
}