using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    public AudioClip entitySpawnSFX;
    private AudioSource audioSource;
    private bool puzzleCompletedTriggered = false;

    [Header("UI Settings")]
    public TextMeshProUGUI hintText;
    public float hintDisplayTime = 3f;

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

    private void Start()
    {
        if (hintText != null)
            hintText.text = "";
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

        ShowDialogue("The clock puzzle is complete! A portal will spawn somewhere in the area.");

        if (RPS != null)
        {
            RPS.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
        }

        if (audioSource != null && puzzleCompleteSFX != null)
        {
            audioSource.PlayOneShot(puzzleCompleteSFX);
        }

        if (currentEntity != null)
        {
            EntityDespawner despawner = currentEntity.GetComponent<EntityDespawner>();

            if (despawner != null)
            {
                despawner.DespawnWithParticles();
            }
            else
            {
                Destroy(currentEntity);
            }

            currentEntity = null;
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
        if (puzzleCompletedTriggered)
        {
            Debug.Log("<color=yellow>StairsEffectManager:</color> Oro triggered, but ignored because puzzle is complete!");
            return;
        }

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
        if (puzzleCompletedTriggered)
        {
            Debug.Log("<color=yellow>StairsEffectManager:</color> Plata triggered, but ignored because puzzle is complete!");
            return;
        }

        if (!TrySetState(StairsTriggerState.Plata))
            return;

        Debug.Log("Clocks reset to 3-o'clock");

        ShowDialogue("The clocks have been reset!");

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
        if (puzzleCompletedTriggered)
        {
            Debug.Log("<color=yellow>StairsEffectManager:</color> Mata triggered, but ignored because puzzle is complete!");
            return;
        }

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

            if (audioSource != null && entitySpawnSFX != null)
            {
                audioSource.PlayOneShot(entitySpawnSFX);
            }

            ShowDialogue("An entity has spawned!");
        }

        canTrigger = true;
    }

    private void ShowDialogue(string message)
    {
        if (hintText != null)
        {
            hintText.text = message;
            StopAllCoroutines();
            StartCoroutine(ClearHintText());
        }
    }

    private IEnumerator ClearHintText()
    {
        yield return new WaitForSeconds(hintDisplayTime);

        if (hintText != null)
            hintText.text = "";
    }
}

public enum StairsTriggerState
{
    None,
    Oro,
    Plata,
    Mata
}