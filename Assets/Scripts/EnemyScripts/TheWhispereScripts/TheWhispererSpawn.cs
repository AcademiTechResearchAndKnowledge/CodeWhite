using UnityEngine;

public class IdleOrClosetEnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Player")]
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private ClosetHidingSystem closetSystem;

    [Header("Spawn Timing")]
    [SerializeField] private float idleSpawnDelay = 10f;
    [SerializeField] private float closetSpawnDelay = 15f;
    [SerializeField] private bool spawnOnlyOnce = true;

    [Header("Idle Check")]
    [SerializeField] private float idleSpeedThreshold = 0.1f;

    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider[] spawnAreas;

    [Header("Spawn Position")]
    [SerializeField] private float groundOffset = 0.2f;

    private float idleTimer;
    private float closetTimer;
    private bool hasSpawned;

    private void Update()
    {
        if (spawnOnlyOnce && hasSpawned)
            return;

        if (playerRb == null)
        {
            Debug.LogWarning("IdleOrClosetEnemySpawner: No player Rigidbody assigned.");
            return;
        }

        CheckIdleTimer();
        CheckClosetTimer();
    }

    private void CheckIdleTimer()
    {
        if (closetSystem != null && closetSystem.InsideCloset)
        {
            idleTimer = 0f;
            return;
        }

        Vector3 flatVelocity = new Vector3(playerRb.linearVelocity.x, 0f, playerRb.linearVelocity.z);

        if (flatVelocity.magnitude <= idleSpeedThreshold)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleSpawnDelay)
            {
                SpawnEnemy();
            }
        }
        else
        {
            idleTimer = 0f;
        }
    }

    private void CheckClosetTimer()
    {
        if (closetSystem != null && closetSystem.InsideCloset)
        {
            closetTimer += Time.deltaTime;

            if (closetTimer >= closetSpawnDelay)
            {
                SpawnEnemy();
            }
        }
        else
        {
            closetTimer = 0f;
        }
    }

    private void SpawnEnemy()
    {
        if (spawnOnlyOnce && hasSpawned)
            return;

        if (enemyPrefab == null)
        {
            Debug.LogWarning("IdleOrClosetEnemySpawner: No enemy prefab assigned.");
            return;
        }

        if (spawnAreas == null || spawnAreas.Length == 0)
        {
            Debug.LogWarning("IdleOrClosetEnemySpawner: No spawn areas assigned.");
            return;
        }

        BoxCollider chosenArea = GetRandomSpawnArea();

        if (chosenArea == null)
        {
            Debug.LogWarning("IdleOrClosetEnemySpawner: Chosen spawn area is null.");
            return;
        }

        Vector3 spawnPos = GetRandomPointInBox(chosenArea);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        hasSpawned = true;
        idleTimer = 0f;
        closetTimer = 0f;

        Debug.Log("Enemy spawned because player stayed still too long or stayed inside closet too long.");
    }

    private BoxCollider GetRandomSpawnArea()
    {
        BoxCollider[] validAreas = System.Array.FindAll(spawnAreas, area => area != null);

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