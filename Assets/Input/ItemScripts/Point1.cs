using UnityEngine;

public class Point1 : MonoBehaviour
{
    [Header("Item")]
    public string itemId = "KeyItem";
    public int amount = 1;

    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Collect();
    }

    private void Collect()
    {
        // Tell PointManager
        if (PointManager.Instance != null)
        {
            PointManager.Instance.AddPoint();
        }
        else
        {
            Debug.LogWarning("No PointManager in scene!");
        }

        Debug.Log($"Picked up {amount} {itemId}");

        // Destroy parent (if exists)
        if (transform.parent != null)
        {
            Destroy(transform.parent.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}