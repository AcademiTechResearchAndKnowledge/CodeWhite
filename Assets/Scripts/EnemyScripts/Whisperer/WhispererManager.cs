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

    [SerializeField]
    private int initialChanceToSpawn = 100;

    [SerializeField]
    private int flashlightLifetime = 10;

    public GameObject[] Spawners;

    bool whispererSpawned = false;
    GameObject spawnedEntity;
    int chanceToSpawn;

    LighterPuzzleManager puzzleManager;
    AudioSource audioSource;

    private void OnEnable()
    {
        // TODO: Implement a method against spam on off flashlight
        // TODO: Refactor Variable Names (and the horrendous chance system, it works but is not reader friendly)
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
        puzzleManager = GameObject.Find("PuzzleManager").GetComponent<LighterPuzzleManager>();
        audioSource = GetComponent<AudioSource>();
        chanceToSpawn = initialChanceToSpawn;
    }

    void rollForTrigger()
    {
        Debug.Log("Checking Trigger: Whisperer");
        // NOTE: add a decrease chance right after despawning
        if (Random.Range(0, 100) < chanceToSpawn && !whispererSpawned)
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
            chanceToSpawn += 10;
        }
    }

    // FLASHLIGHT TRIGGER: Using the flashlight for more than flashlightLifetime initiates rollForTrigger()
    Coroutine spawnTimerRoutine;

    void StartFlashTimer()
    {
        spawnTimerRoutine = StartCoroutine(SpawnTimerRoutine());
    }

    void StopFlashTimer()
    {
        Debug.Log("Flash TIMER STOPPED");
        StopCoroutine(spawnTimerRoutine);
    }

    IEnumerator SpawnTimerRoutine()
    {
        Debug.Log("Flash TIMER STARTED");
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
