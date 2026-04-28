using System.Collections.Generic;
using UnityEngine;

public class LibraryBookSpawner : MonoBehaviour
{
    [Header("Book Prefab")]
    [Tooltip("Drag your SINGLE Base Book Prefab here.")]
    public GameObject baseBookPrefab;

    [Header("Item Data (Scriptable Objects)")]
    [Tooltip("Drag your 3 different ObjectiveItemData ScriptableObjects here.")]
    public ObjectiveItemData signedBookData;
    public ObjectiveItemData unsignedBookData;
    public ObjectiveItemData forgedBookData;

    [Header("Spawn Points")]
    [Tooltip("Drag all your Empty GameObjects here. You need AT LEAST 20.")]
    public List<Transform> spawnPoints;

    private void Start()
    {
        SpawnBooks();
    }

    private void SpawnBooks()
    {
        if (spawnPoints.Count < 20)
        {
            Debug.LogError($"[Book Spawner] Failed! Only {spawnPoints.Count} spawn points. You need at least 20.");
            return;
        }

        if (signedBookData == null || unsignedBookData == null || forgedBookData == null)
        {
            Debug.LogError("[Book Spawner] Missing Item Data!");
            return;
        }

        List<ObjectiveItemData> dataToSpawn = new List<ObjectiveItemData>();

        for (int i = 0; i < 10; i++) dataToSpawn.Add(signedBookData);
        for (int i = 0; i < 5; i++) dataToSpawn.Add(unsignedBookData);
        for (int i = 0; i < 5; i++) dataToSpawn.Add(forgedBookData);

        ShuffleList(dataToSpawn);
        ShuffleList(spawnPoints);

        for (int i = 0; i < 20; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError($"[Book Spawner] FAILED! Spawn Point at index {i} is missing/destroyed!");
                return;
            }

            GameObject newBook = Instantiate(baseBookPrefab, spawnPoints[i].position, spawnPoints[i].rotation, transform);

            ObjectiveItemPickup pickupScript = newBook.GetComponent<ObjectiveItemPickup>();
            LibraryBook visualScript = newBook.GetComponent<LibraryBook>();

            if (pickupScript != null && visualScript != null)
            {
                ObjectiveItemData uniqueItemData = Instantiate(dataToSpawn[i]);

                // -------------------------------------------------------------
                // THE FIX: The Spawner rolls the dice to pick the color!
                // -------------------------------------------------------------
                int randomColorIndex = Random.Range(0, 4); // Rolls 0, 1, 2, or 3
                uniqueItemData.visualIndex = randomColorIndex;

                if (uniqueItemData.bookIcons != null && randomColorIndex < uniqueItemData.bookIcons.Length)
                {
                    uniqueItemData.icon = uniqueItemData.bookIcons[randomColorIndex];
                }

                pickupScript.itemData = uniqueItemData;

                // Tell the book to immediately update its 3D model to match!
                visualScript.selectedVisualIndex = randomColorIndex;
                visualScript.UpdateVisuals();
            }
        }

        Debug.Log("[Book Spawner] Successfully spawned 20 books with truly random colors.");
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}