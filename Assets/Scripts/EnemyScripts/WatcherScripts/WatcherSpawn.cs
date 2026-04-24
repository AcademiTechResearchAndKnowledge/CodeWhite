using UnityEngine;
using System.Collections.Generic;

public class WatcherSpawn : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private PlayerReferences playerRef;

    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Timer")]
    [SerializeField] private float spawnDelay = 300f;
    [SerializeField] private bool spawnOnlyOnce = true;

    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider[] spawnAreas;
    [SerializeField] private LayerMask enemySpawnMask;

    [Header("Spawn Position")]
    [SerializeField] private float groundOffset = 0.5f;

    private float timer;
    private bool hasSpawned;

    private void Start()
    {
        if (playerRef == null)
        {
            playerRef = Object.FindFirstObjectByType<PlayerReferences>();
        }
    }

    private void Update()
    {
        if (spawnOnlyOnce && hasSpawned)
            return;

        timer += Time.deltaTime;

        if (timer >= spawnDelay)
        {
            SpawnEnemy();

            if (spawnOnlyOnce)
                hasSpawned = true;
            else
                timer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("TimedEnemySpawner: No enemy prefab assigned.");
            return;
        }

        if (spawnAreas == null || spawnAreas.Length == 0)
        {
            Debug.LogWarning("TimedEnemySpawner: No spawn areas assigned.");
            return;
        }

        if (playerRef == null)
        {
            Debug.LogWarning("TimedEnemySpawner: Player Reference is missing. Cannot calculate closest spawn.");
            return;
        }

        BoxCollider bestArea = GetBestSpawnArea();

        if (bestArea == null)
        {
            Debug.LogWarning("TimedEnemySpawner: No valid spawn area found on the correct layer.");
            return;
        }

        Vector3 spawnPos = GetRandomPointInBox(bestArea);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Enemy spawned.");
    }

    private BoxCollider GetBestSpawnArea()
    {
        List<BoxCollider> validAreas = new List<BoxCollider>();
        foreach (var area in spawnAreas)
        {
            if (area != null && ((enemySpawnMask.value & (1 << area.gameObject.layer)) != 0))
            {
                validAreas.Add(area);
            }
        }

        if (validAreas.Count == 0)
            return null;

        validAreas.Sort((a, b) =>
        {
            float distA = Vector3.Distance(playerRef.transform.position, a.bounds.center);
            float distB = Vector3.Distance(playerRef.transform.position, b.bounds.center);
            return distA.CompareTo(distB);
        });

        foreach (var area in validAreas)
        {
            if (!area.bounds.Contains(playerRef.transform.position))
            {
                return area;
            }
        }

        return validAreas[validAreas.Count - 1];
    }

    private Vector3 GetRandomPointInBox(BoxCollider box)
    {
        Bounds bounds = box.bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);

        float y = bounds.min.y + groundOffset;

        return new Vector3(randomX, y, randomZ);
    }
}