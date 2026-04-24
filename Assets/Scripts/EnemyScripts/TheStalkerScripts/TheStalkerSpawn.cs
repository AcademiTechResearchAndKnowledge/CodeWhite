using UnityEngine;
using System.Collections.Generic;

public class IdleOrClosetEnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Player")]
    [SerializeField] private PlayerReferences playerRefs;
    [SerializeField] private ClosetHidingSystem closetSystem;

    [Header("Spawn Timing")]
    [SerializeField] private float idleSpawnDelay = 10f;
    [SerializeField] private float closetSpawnDelay = 15f;

    [Header("Idle Check")]
    [SerializeField] private float idleSpeedThreshold = 0.1f;

    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider[] spawnAreas;

    [Header("Spawn Position")]
    [SerializeField] private float groundOffset = 0.2f;

    private float idleTimer;
    private float closetTimer;
    private GameObject activeStalker;

    private void Start()
    {
        playerRefs = FindAnyObjectByType<PlayerReferences>();

        if (playerRefs == null)
        {
            Debug.LogError("IdleOrClosetEnemySpawner: Could not find PlayerReferences in the scene!");
        }
    }

    private void Update()
    {
        if (activeStalker != null)
            return;

        if (playerRefs == null || playerRefs.rb == null)
            return;

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

        Vector3 flatVelocity = new Vector3(playerRefs.rb.linearVelocity.x, 0f, playerRefs.rb.linearVelocity.z);

        if (flatVelocity.magnitude <= idleSpeedThreshold)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleSpawnDelay)
            {
                SpawnEnemy(isClosetSpawn: false);
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
                SpawnEnemy(isClosetSpawn: true);
            }
        }
        else
        {
            closetTimer = 0f;
        }
    }

    private void SpawnEnemy(bool isClosetSpawn)
    {
        if (enemyPrefab == null || spawnAreas == null || spawnAreas.Length == 0)
        {
            Debug.LogWarning("Missing Prefab or Spawn Areas.");
            return;
        }

        BoxCollider chosenArea = GetBestSpawnArea();

        if (chosenArea == null) return;

        Vector3 spawnPos = GetRandomPointInBox(chosenArea);

        activeStalker = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        StalkerFollowScript stalkerScript = activeStalker.GetComponent<StalkerFollowScript>();

        if (stalkerScript != null)
        {
            if (isClosetSpawn)
            {
                stalkerScript.InitializeForCloset(playerRefs, closetSystem);
            }
            else
            {
                stalkerScript.InitializeForIdle(playerRefs, idleSpeedThreshold);
            }
        }
        else
        {
            Debug.LogError("Enemy prefab is missing the StalkerFollowScript!");
        }

        idleTimer = 0f;
        closetTimer = 0f;

        Debug.Log($"Stalker spawned. Reason: {(isClosetSpawn ? "Closet" : "Idle")}");
    }

    private BoxCollider GetBestSpawnArea()
    {
        List<BoxCollider> validAreas = new List<BoxCollider>();
        foreach (var area in spawnAreas)
        {
            if (area != null)
            {
                validAreas.Add(area);
            }
        }

        if (validAreas.Count == 0)
            return null;

        validAreas.Sort((a, b) =>
        {
            float distA = Vector3.Distance(playerRefs.transform.position, a.bounds.center);
            float distB = Vector3.Distance(playerRefs.transform.position, b.bounds.center);
            return distA.CompareTo(distB);
        });

        foreach (var area in validAreas)
        {
            if (!area.bounds.Contains(playerRefs.transform.position))
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