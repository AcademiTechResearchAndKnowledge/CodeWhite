using UnityEngine;

public class StairsEffectManager : MonoBehaviour
{
    public static StairsEffectManager Instance;

    [Header("Player")]


    [Header("Entity")]
    public GameObject entityPrefab;
    private GameObject currentEntity;

    public int oroStepCount = 0;


    private void Awake()
    {
        Instance = this;
    }


    public void TriggerOro()
    {
        Debug.Log("Anxiety reduced by 5%");

        oroStepCount++;

        if (oroStepCount >= 3 && currentEntity != null)
        {
            Destroy(currentEntity);
            currentEntity = null;
            oroStepCount = 0;
        }
    }

    public void TriggerPlata()
    {
        Debug.Log("Clocks reset to 3-o'clock");

        foreach (AnalogClock clock in AnalogClock.allClocks)
        {
            clock.hours = 3;
            clock.minutes = 0;
            clock.UpdateClockVisuals();  
        }


        AnalogClock.puzzleDone = false;
    }




    public void TriggerMata(Vector3 spawnPosition)
    {
        if (currentEntity == null)
        {
            currentEntity = Instantiate(entityPrefab, spawnPosition, Quaternion.identity);

            Debug.Log("Entity spawned, Anxiety increased by 2%");
        }
        else
        {
            Debug.Log("Anxiety increased by 2%");
        }
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