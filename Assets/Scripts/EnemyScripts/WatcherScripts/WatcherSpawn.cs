using UnityEngine;

public class WatcherSpawn : MonoBehaviour
{
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

        BoxCollider validArea = GetRandomValidSpawnArea();

        if (validArea == null)
        {
            Debug.LogWarning("TimedEnemySpawner: No valid spawn area found on the correct layer.");
            return;
        }

        Vector3 spawnPos = GetRandomPointInBox(validArea);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Enemy spawned.");
    }

    private BoxCollider GetRandomValidSpawnArea()
    {
        BoxCollider[] validAreas = System.Array.FindAll(spawnAreas, area =>
            area != null && ((enemySpawnMask.value & (1 << area.gameObject.layer)) != 0)
        );

        if (validAreas.Length == 0)
            return null;

        return validAreas[Random.Range(0, validAreas.Length)];
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