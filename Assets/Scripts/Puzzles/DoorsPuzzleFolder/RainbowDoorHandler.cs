using UnityEngine;

public class RainbowDoorInteractable : Interactable
{
    [Header("References")]
   // public GameObject eye;  // assign the Eye object behind the door

    [Header("Debug/Entity")]
    public float entityTriggerDistance = 10f; //distance to trigger entity

    private bool isOpened = false;

    void Update()
    {
        
        if (!isOpened && PlayerExists())
        {
            float distance = Vector3.Distance(PlayerPosition(), transform.position);
            if (distance <= entityTriggerDistance)
            {
                TriggerEntity();
            }
        }
    }

    public override void Interact()
    {
        if (isOpened)
        {
            Debug.Log("Rainbow door is already open.");
            return;
        }

        if (DoorPuzzleHandler.instance.hasKey)
        {
            Debug.Log("Rainbow door opened!");
            isOpened = true;
            
            /*
            if (eye != null)
            {
                eye.SetActive(true);
                Debug.Log("The eye appears behind the door...");
            }
            */
        }
        else
        {
            Debug.Log("The rainbow door is locked. You need a key.");
        }
    }

    void TriggerEntity()
    {
        Debug.Log("Entity has been triggered and is approaching the player...");
        // add entity stuff here lmao
    }

    Vector3 PlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform.position : Vector3.zero;
    }

    bool PlayerExists()
    {
        return GameObject.FindGameObjectWithTag("Player") != null;
    }
}