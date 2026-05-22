using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhispererManager : MonoBehaviour
{
    public delegate void OnWhisperFlicker();
    public static event OnWhisperFlicker onWhisperFlicker;

    public delegate void OnWhispererSpawned();
    public static event OnWhispererSpawned onWhispererSpawned;

    // A public lock state that lights can check
    public static bool IsWhispererActive { get; private set; } = false;

    [Header("Player Reference")]
    [SerializeField] private PlayerReferences playerRefs;

    [Header("Whisperer Settings")]
    public int Stage = 1;
    public GameObject Entity;

    [SerializeField]
    private AudioClip Whisper;

    [Header("Trigger Chances Settings")]
    [SerializeField]
    [Tooltip("Minimum base percentage chance to trigger a stage.")]
    private int minBaseChance = 5;
    [SerializeField]
    [Tooltip("Maximum base percentage chance to trigger a stage.")]
    private int maxBaseChance = 20;
    [SerializeField]
    [Tooltip("How much the chance increases every time a light fails to trigger a stage.")]
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

        // Initialize the first random chance
        resetState();

        if (playerRefs == null)
        {
            playerRefs = FindAnyObjectByType<PlayerReferences>();
        }
    }

    private void Update()
    {
        // If the whisperer was spawned, but the GameObject is now null (destroyed)
        if (whispererSpawned && spawnedEntity == null)
        {
            whispererSpawned = false;
            IsWhispererActive = false; // Unlock the lights!
            Debug.Log("Whisperer has despawned. Lights are unlocked.");
        }
    }

    void rollForTrigger()
    {
        if (whispererSpawned)
            return;

        // Roll 1 to 100 for true percentage math
        if (Random.Range(1, 101) <= chanceToSpawn)
        {
            switch (Stage)
            {
                case 1:
                    audioSource.clip = Whisper;
                    audioSource.Play();
                    Stage++; // Safely increment inside the case
                    break;
                case 2:
                    onWhisperFlicker?.Invoke();
                    Stage++; // Safely increment inside the case
                    break;
                case 3:
                    whispererSpawned = true;
                    IsWhispererActive = true; // Lock the lights!
                    Spawn();
                    onWhispererSpawned?.Invoke();

                    // Reset the loop for the next time!
                    resetState();
                    break;
            }
        }
        else
        {
            // Failed the roll. Increase the odds for the next light they turn on.
            chanceToSpawn += chanceIncrementPerFail;
        }
    }

    void StartFlashTimer()
    {
        spawnTimerRoutine = StartCoroutine(SpawnTimerRoutine());
    }

    void StopFlashTimer()
    {
        if (spawnTimerRoutine != null)
        {
            StopCoroutine(spawnTimerRoutine);
        }
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
        IsWhispererActive = false; // Unlock the lights!

        if (spawnedEntity != null)
        {
            Destroy(spawnedEntity);
        }
    }

    void resetState()
    {
        Stage = 1;
        // Generate a completely random starting percentage between the min and max
        chanceToSpawn = Random.Range(minBaseChance, maxBaseChance + 1);
    }
}