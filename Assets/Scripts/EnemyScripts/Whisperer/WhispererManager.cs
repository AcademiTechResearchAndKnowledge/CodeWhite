using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhispererManager : MonoBehaviour
{
    public delegate void OnWhisperFlicker();
    public static event OnWhisperFlicker onWhisperFlicker;

    [Header("Player Reference")]
    [SerializeField] private PlayerReferences playerRefs;

    [Header("Whisperer Settings")]
    public int Stage = 1;
    public GameObject Entity;

    [SerializeField]
    private AudioClip Whisper;

    [Header("Trigger Chances Settings")]
    [SerializeField]
    private int initialChanceToSpawn = 100;
    [SerializeField]
    private int chanceIncrementPerFail = 10;

    [Header("Timer Settings")]
    [SerializeField]
    private int flashlightLifetime = 10;

    [Header("Spawn Areas")]
    public BoxCollider[] Spawners;

    bool whispererSpawned = false;
    GameObject spawnedEntity;
    int chanceToSpawn;

    AudioSource audioSource;
    Coroutine spawnTimerRoutine;

    private void OnEnable()
    {
        Flashlight.onFlashlightOn += StartFlashTimer;
        Flashlight.onFlashlightOff += StopFlashTimer;
        SimpleCandleInteract.onSimpleCandleLit += rollForTrigger;
    }

    private void OnDisable()
    {
        Flashlight.onFlashlightOn -= StartFlashTimer;
        Flashlight.onFlashlightOff -= StopFlashTimer;
        SimpleCandleInteract.onSimpleCandleLit -= rollForTrigger;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        chanceToSpawn = initialChanceToSpawn;

        if (playerRefs == null)
        {
            playerRefs = FindAnyObjectByType<PlayerReferences>();
        }
    }

    void rollForTrigger()
    {
        if (whispererSpawned)
            return;

        if (Random.Range(0, 100) < chanceToSpawn)
        {
            switch (Stage)
            {
                case 1:
                    audioSource.clip = Whisper;
                    audioSource.Play();
                    break;
                case 2:
                    onWhisperFlicker?.Invoke();
                    break;
                case 3:
                    whispererSpawned = true;
                    Spawn();
                    resetState();
                    break;
            }

            Stage++;
        }
        else
        {
            chanceToSpawn += chanceIncrementPerFail;
        }
    }

    void StartFlashTimer()
    {
        spawnTimerRoutine = StartCoroutine(SpawnTimerRoutine());
    }

    void StopFlashTimer()
    {
        StopCoroutine(spawnTimerRoutine);
    }

    IEnumerator SpawnTimerRoutine()
    {
        yield return new WaitForSeconds(flashlightLifetime);
        rollForTrigger();
        StartFlashTimer();
    }

    [ContextMenu("Spawn Whisperer")]
    public void Spawn()
    {
        if (Entity == null || Spawners == null || Spawners.Length == 0 || playerRefs == null)
        {
            return;
        }

        BoxCollider bestSpawner = GetBestSpawner();

        if (bestSpawner == null)
        {
            return;
        }

        spawnedEntity = Instantiate(Entity, bestSpawner.transform.position, Quaternion.identity);
    }

    private BoxCollider GetBestSpawner()
    {
        try
        {
            if (playerRefs == null) return null;

            Vector3 playerPos = playerRefs.transform.position;

            BoxCollider bestValidSpawner = null;
            float closestValidDist = float.MaxValue;

            BoxCollider furthestFallback = null;
            float furthestDist = float.MinValue;

            foreach (var spawner in Spawners)
            {
                if (spawner == null) continue;

                float dist = Vector3.Distance(playerPos, spawner.transform.position);

                if (!spawner.bounds.Contains(playerPos))
                {
                    if (dist < closestValidDist)
                    {
                        closestValidDist = dist;
                        bestValidSpawner = spawner;
                    }
                }

                if (dist > furthestDist)
                {
                    furthestDist = dist;
                    furthestFallback = spawner;
                }
            }

            if (bestValidSpawner != null)
            {
                return bestValidSpawner;
            }
            else
            {
                return furthestFallback;
            }
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    [ContextMenu("Despawn Whisperer")]
    public void Despawn()
    {
        whispererSpawned = false;
        if (spawnedEntity != null)
        {
            Destroy(spawnedEntity);
        }
    }

    void resetState()
    {
        Stage = 1;
        chanceToSpawn = initialChanceToSpawn;
    }
}