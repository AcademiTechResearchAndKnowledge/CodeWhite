using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class PlayerActionDetector : MonoBehaviour
{
    private PlayerInput controls;
    private bool hasMadeUnnecessaryAction = false;
    private bool hasTriggeredStart = false;

    public Transform PlayerLoc;
    public EntityAi[] npc;
    public int totalNPC = 4;
    public GameObject npcParentHandler;

    void Awake()
    {
        controls = new PlayerInput();
    }

    private void Update()
    {
        if(hasTriggeredStart)
        {
            
        }
    }

    void OnEnable()
    {
        controls.Enable();

        controls.OnFoot.Sprint.performed += OnRun;
        controls.OnFoot.Jump.performed += OnJump;

    }

    void OnDisable()
    {
        controls.OnFoot.Sprint.performed += OnRun;
        controls.OnFoot.Jump.performed -= OnJump;


        controls.Disable();
    }

    void OnRun(InputAction.CallbackContext ctx)
    {
        TriggerAction("Running");
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        TriggerAction("Jumping");
    }

    void OnInteract(InputAction.CallbackContext ctx)
    {
        TriggerAction("Interacting");
    }

    public void OnTriggered(Collider other)
    {
        
            Debug.Log("trigger npc follow");
            hasTriggeredStart = true;
            for(int i = 0; i < totalNPC; i++)
            {
                npc[i].enabled = true;
            }
        
    }


    public void TriggerAction(string action)
    {
        if (!hasTriggeredStart) return;
        if (hasMadeUnnecessaryAction) return;

        hasMadeUnnecessaryAction = true;

        Debug.Log("Unnecessary Action Detected: " + action);
        for(int i = 0; i < totalNPC; i++)
            {
                npc[i].movePositionTransform = PlayerLoc;
            }


    }
}