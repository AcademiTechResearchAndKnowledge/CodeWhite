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
        // Safety Check 1: Do we have enough spawn points?
        if (spawnPoints.Count < 20)
        {
            Debug.LogError($"[Book Spawner] Failed! Only {spawnPoints.Count} spawn points. You need at least 20.");
            return;
        }

        // Safety Check 2: Are the ScriptableObjects assigned?
        if (signedBookData == null || unsignedBookData == null || forgedBookData == null)
        {
            Debug.LogError("[Book Spawner] Missing Item Data! Please assign the ScriptableObjects in the inspector.");
            return;
        }

        // 1. Create a list of the specific Item Data we need to distribute
        List<ObjectiveItemData> dataToSpawn = new List<ObjectiveItemData>();

        for (int i = 0; i < 10; i++) dataToSpawn.Add(signedBookData);
        for (int i = 0; i < 5; i++) dataToSpawn.Add(unsignedBookData);
        for (int i = 0; i < 5; i++) dataToSpawn.Add(forgedBookData);

        // 2. Shuffle the data and the spawn points
        ShuffleList(dataToSpawn);
        ShuffleList(spawnPoints);

        // 3. Spawn the base books and inject their specific cloned Item Data
        for (int i = 0; i < 20; i++)
        {
            // Safety Check 3: Is this specific spawn point empty?
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
                // Create a totally unique clone of the ScriptableObject for this specific book
                ObjectiveItemData uniqueItemData = Instantiate(dataToSpawn[i]);

                // Stamp it with the color index that the visual script randomly picked
                int chosenIndex = visualScript.selectedVisualIndex;
                uniqueItemData.visualIndex = chosenIndex;

                // --- NEW: DYNAMIC ICON STAMPING ---
                // Check if the array exists and the index is safe to use
                if (uniqueItemData.bookIcons != null && chosenIndex < uniqueItemData.bookIcons.Length)
                {
                    // Overwrite the main UI icon with the colored version
                    uniqueItemData.icon = uniqueItemData.bookIcons[chosenIndex];
                }
                // ----------------------------------

                // Give this unique data to the pickup script
                pickupScript.itemData = uniqueItemData;
            }
            else
            {
                Debug.LogWarning("[Book Spawner] The base prefab is missing the ObjectiveItemPickup or LibraryBook script!");
            }
        }

        Debug.Log("[Book Spawner] Successfully spawned 20 books with unique visual and UI data.");
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