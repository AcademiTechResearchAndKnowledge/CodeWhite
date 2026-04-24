using System.Collections.Generic;
using UnityEngine;

public class LibraryBookSpawner : MonoBehaviour
{
    [Header("Book Prefabs")]
    [Tooltip("Drag your 3 different Book Prefabs here.")]
    public GameObject signedBookPrefab;
    public GameObject blankBookPrefab;
    public GameObject forgedBookPrefab;

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
            Debug.LogError($"[Book Spawner] Not enough spawn points! You only have {spawnPoints.Count}, but you need at least 20. Please add more.");
            return;
        }

        List<GameObject> booksToSpawn = new List<GameObject>();

        for (int i = 0; i < 10; i++) booksToSpawn.Add(signedBookPrefab);
        for (int i = 0; i < 5; i++) booksToSpawn.Add(blankBookPrefab);
        for (int i = 0; i < 5; i++) booksToSpawn.Add(forgedBookPrefab);

        ShuffleList(booksToSpawn);

        ShuffleList(spawnPoints);

        for (int i = 0; i < 20; i++)
        {
            Instantiate(booksToSpawn[i], spawnPoints[i].position, spawnPoints[i].rotation, transform);
        }

        Debug.Log("[Book Spawner] Successfully spawned 10 Signed, 5 Blank, and 5 Forged books at random locations.");
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