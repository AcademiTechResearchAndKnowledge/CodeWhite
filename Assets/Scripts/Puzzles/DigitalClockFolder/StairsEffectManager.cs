using System.Collections.Generic;
using UnityEngine;

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

    public void Update()
    {
        foreach (AnalogClock clock in allAnalaogClocks)
        {
            if (clock.allPuzzleDone == true)
            {
                RPS.SpawnPortalRandom();
            }
        }
    }

    private void Awake()
    {
        Instance = this;
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

        Debug.Log("Anxiety reduced by 5%");

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
        }

        AnalogClock.puzzleDone = false;

        canTrigger = true;
    }

    public void TriggerMata(Vector3 spawnPosition)
    {
        if (!TrySetState(StairsTriggerState.Mata))
            return;

        if (currentEntity == null)
        {
            currentEntity = Instantiate(entityPrefab, spawnPosition, Quaternion.identity);
            Debug.Log("Entity spawned, Anxiety increased by 2%");
        }
        else
        {
            Debug.Log("Anxiety increased by 2%");
        }

        canTrigger = true;
    }

    public void OnEntityCaughtPlayer()
    {
        Debug.Log("You have been caught, Axiety increased by 40%");

        if (currentEntity != null)
        {
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