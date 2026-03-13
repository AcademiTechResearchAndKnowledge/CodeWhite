using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionDetector : MonoBehaviour
{
    private PlayerInput controls;
    private bool hasMadeUnnecessaryAction = false;

    void Awake()
    {
        controls = new PlayerInput();
    }

    void OnEnable()
    {
        controls.Enable();

        //controls.OnFoot.Run.performed += OnRun;
        controls.OnFoot.Jump.performed += OnJump;

    }

    void OnDisable()
    {
        //controls.OnFoot.Run.performed -= OnRun;
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

    void TriggerAction(string action)
    {
        if (hasMadeUnnecessaryAction) return;

        hasMadeUnnecessaryAction = true;

        Debug.Log("Unnecessary Action Detected: " + action);


    }
}