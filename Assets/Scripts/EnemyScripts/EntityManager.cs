using System.Collections;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public delegate void OnWhisperFlicker();
    public static event OnWhisperFlicker onWhisperFlicker;

    [SerializeField]
    private AudioClip WhispererWhisper;

    [SerializeField]
    private int flashlightLifetime = 10;

    WhispererSpawner spawner;
    PuzzleManager puzzleManager;
    AudioSource audioSource;
    int whispererStage = 1;

    private void OnEnable()
    {
        // TODO: Implement a method against spam on off flashlight
        // TODO: Refactor Code
        Flashlight.onFlashlightOn += StartSpawnTimer;
        Flashlight.onFlashlightOff += StopSpawnTimer;
        CandleInteract.onCandleLit += TriggerWhisperer;
    }

    private void OnDisable()
    {
        Flashlight.onFlashlightOn -= StartSpawnTimer;
        Flashlight.onFlashlightOff -= StopSpawnTimer;
        CandleInteract.onCandleLit -= TriggerWhisperer;
    }

    private void Awake()
    {
        spawner = GameObject.Find("WhispererSpawner").GetComponent<WhispererSpawner>();
        puzzleManager = GameObject.Find("PuzzleManager").GetComponent<PuzzleManager>();
        audioSource = GetComponent<AudioSource>();
    }

    bool whispererSpawned = false;

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
        TriggerWhisperer();
        StartSpawnTimer();
    }

    void TriggerWhisperer()
    {
        Debug.Log("Checking Trigger: Whisperer");
        // NOTE: add a decrease chance right after despawning
        int triggerChance = puzzleManager.candlesLit;
        if (Random.Range(0, 10) < 10 && !whispererSpawned)
        {
            switch (whispererStage)
            {
                case 1:
                    // Play Whispering Audio
                    audioSource.clip = WhispererWhisper;
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
                    spawner.SpawnWhisperer();
                    whispererStage = 0;
                    break;
            }

            whispererStage++;
        }
    }

    void DespawnWhisperer()
    {
        spawner.DespawnWhisperer();
    }
}
