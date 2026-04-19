using UnityEngine;

public class DoorPuzzleHandler : MonoBehaviour
{
    public static DoorPuzzleHandler instance;

    public bool hasKey = false;
    public bool entityTriggered = false;

    private void Awake()
    {
        instance = this;
    }

    public void TriggerEntity()
    {
        if (!entityTriggered)
        {
            entityTriggered = true;
            Debug.Log("Entity has been triggered and is approaching the player...");
        }
    }

    public void IncreaseEntitySpeed()
    {
        Debug.Log("Entity speed increased! It is rushing the player!");
    }
}