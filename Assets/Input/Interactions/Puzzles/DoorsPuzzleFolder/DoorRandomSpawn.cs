using UnityEngine;
using System.Collections.Generic;

public class doorsGen : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject blackDoorPrefab;
    public GameObject rainbowDoorPrefab;
    public GameObject whiteDoorPrefab;
    public GameObject keyPrefab;

    [Header("Parents")]
    public Transform doorsParent;   // assign empty object to group doors
    public Transform keysParent;    // optional, for keys

    [Header("Floor")]
    public Transform floor;

    [Header("Generation Settings")]
    public int totalDoors = 20; // total number of doors including special doors
    public float spawnAreaSize = 50f;
    public float minDistanceBetweenDoors = 5f;

    private List<Vector3> usedPositions = new List<Vector3>();
    private float floorTop;

    void Start()
    {
        Collider floorCollider = floor.GetComponent<Collider>();
        floorTop = floorCollider.bounds.max.y;

        SpawnDoors();
        SpawnKey();
    }

    void SpawnDoors()
    {
        // Randomly pick which indices will be special doors
        int rainbowIndex = Random.Range(0, totalDoors);
        int whiteIndex = Random.Range(0, totalDoors);

        // Make sure rainbow and white are not the same
        while (whiteIndex == rainbowIndex)
        {
            whiteIndex = Random.Range(0, totalDoors);
        }

        for (int i = 0; i < totalDoors; i++)
        {
            Vector3 randomXZ = GetValidRandomPosition();
            if (randomXZ == Vector3.zero) continue; // skip if not valid

            GameObject prefabToSpawn = blackDoorPrefab;

            if (i == rainbowIndex) prefabToSpawn = rainbowDoorPrefab;
            else if (i == whiteIndex) prefabToSpawn = whiteDoorPrefab;

            float randomRotation = Random.Range(0f, 360f);

            // Instantiate at floorTop first
            GameObject door = Instantiate(
                prefabToSpawn,
                new Vector3(randomXZ.x, floorTop, randomXZ.z),
                Quaternion.Euler(0, randomRotation, 0),
                doorsParent
            );

            // Adjust Y so bottom touches floor
            Collider doorCollider = door.GetComponent<Collider>();
            if (doorCollider != null)
            {
                float bottomOffset = doorCollider.bounds.min.y - door.transform.position.y;
                door.transform.position = new Vector3(door.transform.position.x, floorTop - bottomOffset, door.transform.position.z);
            }

            usedPositions.Add(randomXZ);
        }

        Debug.Log("Doors spawned! Rainbow and White doors included.");
    }

    void SpawnKey()
    {
        Vector3 randomXZ = GetValidRandomPosition();
        if (randomXZ == Vector3.zero) return;

        GameObject key = Instantiate(
            keyPrefab,
            new Vector3(randomXZ.x, floorTop, randomXZ.z),
            Quaternion.identity,
            keysParent
        );

        Collider keyCollider = key.GetComponent<Collider>();
        if (keyCollider != null)
        {
            float bottomOffset = keyCollider.bounds.min.y - key.transform.position.y;
            key.transform.position = new Vector3(key.transform.position.x, floorTop - bottomOffset, key.transform.position.z);
        }

        Debug.Log("Key spawned at random location!");
    }

    Vector3 GetValidRandomPosition()
    {
        int attempts = 0;
        while (attempts < 100)
        {
            attempts++;
            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaSize, spawnAreaSize),
                0,
                Random.Range(-spawnAreaSize, spawnAreaSize)
            );

            bool farEnough = true;
            foreach (Vector3 used in usedPositions)
            {
                if (Vector3.Distance(pos, used) < minDistanceBetweenDoors)
                {
                    farEnough = false;
                    break;
                }
            }

            if (farEnough) return pos;
        }

        Debug.LogWarning("Failed to find valid spawn position after 100 attempts");
        return Vector3.zero;
    }
}