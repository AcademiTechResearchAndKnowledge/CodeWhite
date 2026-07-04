using UnityEngine;

public class WhiteDoorInteractable : Interactable
{
    [Header("Entity Settings")]
    public float triggerDistance = 5f; // how close player needs to be
    private bool triggered = false;

    void Update()
    {
        if (triggered) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= triggerDistance)
        {
            triggered = true;
            TriggerEntitySpeedIncrease();
        }
    }

    public override void Interact()
    {
        Debug.Log("White door interacted");
        // add other this here if meron or possible or kahit ano lmao
    }

    void TriggerEntitySpeedIncrease()
    {
        Debug.Log("Entity speed increased! It is rushing the player!");
        // enitiy code here type shiii
    }
}