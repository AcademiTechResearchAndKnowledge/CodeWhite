using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Item Table")]
    public ItemDatabase itemDatabase; 
    public GameObject itemPrefab;

    [Header("Spawn Settings")]
    public float spawnHeightOffset = 1f;

    public void SpawnRandomItem(Vector3 spawnPosition)
    {
        if (itemDatabase == null || itemDatabase.items.Length == 0 || itemPrefab == null)
        {
            Debug.LogError("ItemDatabase is empty or prefab missing!");
            return;
        }

        ItemData randomItem = itemDatabase.items[Random.Range(0, itemDatabase.items.Length)];

        Vector3 spawnPos = spawnPosition + new Vector3(0f, spawnHeightOffset, 0f);
        GameObject spawnedItem = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        spawnedItem.name = randomItem.itemName;

        SpriteRenderer sr = spawnedItem.GetComponent<SpriteRenderer>();
        if (randomItem.sprite != null)
            sr.sprite = randomItem.sprite;
        else
            sr.color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);

        Debug.Log($"Spawned: {randomItem.itemName}");
    }
}