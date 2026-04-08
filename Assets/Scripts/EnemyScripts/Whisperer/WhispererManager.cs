using System.Collections;
using UnityEngine;

public class WhispererManager : MonoBehaviour
{
    public delegate void OnWhisperFlicker();
    public static event OnWhisperFlicker onWhisperFlicker;

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

    public GameObject[] Spawners;

    bool whispererSpawned = false;
    GameObject spawnedEntity;
    int chanceToSpawn;

    AudioSource audioSource;

    private void OnEnable()
    {
        Flashlight.onFlashlightOn += StartFlashTimer;
        Flashlight.onFlashlightOff += StopFlashTimer;
        CandleInteract.onCandleLit += rollForTrigger;
    }

    private void OnDisable()
    {
        Flashlight.onFlashlightOn -= StartFlashTimer;
        Flashlight.onFlashlightOff -= StopFlashTimer;
        CandleInteract.onCandleLit -= rollForTrigger;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        chanceToSpawn = initialChanceToSpawn;
    }

    void rollForTrigger()
    {
        Debug.Log("Checking Trigger: Whisperer");
        if (whispererSpawned)
            return;

        // NOTE: add a decrease chance right after despawning
        if (Random.Range(0, 100) < chanceToSpawn)
        {
            switch (Stage)
            {
                case 1:
                    // Play Whispering Audio
                    audioSource.clip = Whisper;
                    audioSource.Play();

                    Debug.Log("Whisperer whispers to you");

                    break;
                case 2:
                    // Flicker Lights
                    onWhisperFlicker?.Invoke();
                    Debug.Log("Lights flicker around you");
                    break;
                case 3:
                    // Spawn Whisperer
                    Debug.Log("Whisperer is now here");

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

        Debug.Log("Chance to spawn: " + chanceToSpawn);
    }

    // FLASHLIGHT TRIGGER: Using the flashlight for more than flashlightLifetime initiates rollForTrigger()
    Coroutine spawnTimerRoutine;

    void StartFlashTimer()
    {
        Debug.Log("Flash TIMER STARTED");
        spawnTimerRoutine = StartCoroutine(SpawnTimerRoutine());
    }

    void StopFlashTimer()
    {
        Debug.Log("Flash TIMER STOPPED");
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
        // Spawn in a random predetermined area (location of its children)
        Transform spawner = Spawners[Random.Range(0, Spawners.Length)].transform;
        spawnedEntity = Instantiate(Entity, spawner.position, Quaternion.identity);
    }

    [ContextMenu("Despawn Whisperer")]
    public void Despawn()
    {
        whispererSpawned = false;
        Destroy(spawnedEntity);
    }

    void resetState()
    {
        Stage = 1;
        chanceToSpawn = initialChanceToSpawn;
    }
}
