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
    private int flashlightLifetime = 10;

    public GameObject[] Spawners;

    bool whispererSpawned = false;

    LighterPuzzleManager puzzleManager;
    AudioSource audioSource;

    private void OnEnable()
    {
        // TODO: Implement a method against spam on off flashlight
        // TODO: Refactor Variable Names (and the horrendous chance system, it works but is not reader friendly)
        // TODO: Add Despawn Mechanic and reduce chance to spawn after spawning (most likely it'll be high after spawning)
        Flashlight.onFlashlightOn += StartSpawnTimer;
        Flashlight.onFlashlightOff += StopSpawnTimer;
        CandleInteract.onCandleLit += rollForTrigger;
    }

    private void OnDisable()
    {
        Flashlight.onFlashlightOn -= StartSpawnTimer;
        Flashlight.onFlashlightOff -= StopSpawnTimer;
        CandleInteract.onCandleLit -= rollForTrigger;
    }

    private void Awake()
    {
        puzzleManager = GameObject.Find("PuzzleManager").GetComponent<LighterPuzzleManager>();
        audioSource = GetComponent<AudioSource>();
    }

    void rollForTrigger()
    {
        Debug.Log("Checking Trigger: Whisperer");
        // NOTE: add a decrease chance right after despawning
        int triggerChance = puzzleManager.candlesLit;
        if (Random.Range(0, 10) < 10 && !whispererSpawned)
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
                    Stage = 0;
                    break;
            }

            Stage++;
        }
    }

    // FLASHLIGHT TRIGGER: Using the flashlight for more than flashlightLifetime initiates rollForTrigger()
    Coroutine spawnTimerRoutine;

    void StartSpawnTimer()
    {
        spawnTimerRoutine = StartCoroutine(SpawnTimerRoutine());
    }

    void StopSpawnTimer()
    {
        Debug.Log("Flash TIMER STOPPED");
        StopCoroutine(spawnTimerRoutine);
    }

    IEnumerator SpawnTimerRoutine()
    {
        Debug.Log("Flash TIMER STARTED");
        yield return new WaitForSeconds(flashlightLifetime);
        rollForTrigger();
        StartSpawnTimer();
    }


    public void Spawn()
    {
        // Spawn in a random predetermined area (location of its children)
        Transform spawner = Spawners[Random.Range(0, Spawners.Length)].transform;
        Instantiate(Entity, spawner.position, Quaternion.identity);
    }

    public void Despawn()
    {
    }
}
