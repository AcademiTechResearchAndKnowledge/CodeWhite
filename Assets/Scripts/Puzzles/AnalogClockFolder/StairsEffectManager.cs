using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class StairsEffectManager : MonoBehaviour
{
    public static StairsEffectManager Instance;

    public RandomPortalSpawner RPS;
    public List<AnalogClock> allAnalaogClocks;

    public GameObject entityPrefab;
    private GameObject currentEntity;

    public int oroStepCount = 0;

    private StairsTriggerState currentState = StairsTriggerState.None;
    private bool canTrigger = true;

    private PlayerStats playerStats;

    [Header("Audio Settings")]
    public AudioClip puzzleCompleteSFX;
    private AudioSource audioSource;
    private bool puzzleCompletedTriggered = false;

    private void Awake()
    {
        Instance = this;

        if (RPS == null)
            RPS = FindFirstObjectByType<RandomPortalSpawner>();

        playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats == null)
            Debug.LogWarning("PlayerStats NOT FOUND in scene!");

        audioSource = GetComponent<AudioSource>();
    }

    public void Update()
    {
        if (puzzleCompletedTriggered) return;

        foreach (AnalogClock clock in allAnalaogClocks)
        {
            if (clock.allPuzzleDone == true)
            {
                HandlePuzzleCompletion();
                break;
            }
        }
    }

    private void HandlePuzzleCompletion()
    {
        puzzleCompletedTriggered = true;

        if (RPS != null)
        {
            RPS.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
        }

        if (audioSource != null && puzzleCompleteSFX != null)
        {
            audioSource.PlayOneShot(puzzleCompleteSFX);
        }
    }

    private bool TrySetState(StairsTriggerState newState)
    {
        if (!canTrigger)
            return false;

        if (currentState == newState)
            return false;

        currentState = newState;
        canTrigger = false;

        return true;
    }

    public void TriggerOro()
    {
        if (!TrySetState(StairsTriggerState.Oro))
            return;

        if (playerStats != null)
        {
            playerStats.SubtractStat(StatType.ANX, 5f);
            Debug.Log("Anxiety reduced by 5% → Current: " + playerStats.Anxiety);
        }

        if (currentEntity != null)
        {
            oroStepCount++;
        }

        if (oroStepCount >= 3 && currentEntity != null)
        {
            Destroy(currentEntity);
            currentEntity = null;
            oroStepCount = 0;
        }

        canTrigger = true;
    }

    public void TriggerPlata()
    {
        if (!TrySetState(StairsTriggerState.Plata))
            return;

        Debug.Log("Clocks reset to 3-o'clock");

        foreach (AnalogClock clock in AnalogClock.allClocks)
        {
            clock.hours = 3;
            clock.minutes = 0;
            clock.UpdateClockVisuals();
            clock.allPuzzleDone = false;
        }

        AnalogClock.puzzleDone = false;

        puzzleCompletedTriggered = false;

        canTrigger = true;
    }

    public void TriggerMata(Vector3 spawnPosition)
    {
        if (!TrySetState(StairsTriggerState.Mata))
            return;

        if (playerStats != null)
        {
            playerStats.AddStat(StatType.ANX, 2f);
            Debug.Log("Anxiety increased by 2% → Current: " + playerStats.Anxiety);
        }

        if (currentEntity == null)
        {
            currentEntity = Instantiate(entityPrefab, spawnPosition, Quaternion.identity);
        }

        canTrigger = true;
    }

    public void OnEntityCaughtPlayer()
    {
        if (playerStats != null)
        {
            playerStats.AddStat(StatType.ANX, 40f);
            Debug.Log("Caught! Anxiety increased → Current: " + playerStats.Anxiety);
        }

        if (currentEntity != null)
        {
            foreach (AnalogClock clock in AnalogClock.allClocks)
            {
                clock.hours = 3;
                clock.minutes = 0;
                clock.UpdateClockVisuals();
            }

            Destroy(currentEntity);
            currentEntity = null;
        }
    }
}

public enum StairsTriggerState
{
    None,
    Oro,
    Plata,
    Mata
}