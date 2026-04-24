using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerActionDetector : MonoBehaviour
{
    private PlayerInput controls;
    public Transform playerRoot;
    public Transform playerStartPoint;
    public EntityAi[] npc;

    private int current_Area = -1;
    private bool isResetting = false;
    private Rigidbody playerRb;

    private void Awake()
    {
        controls = new PlayerInput();
        if (playerRoot != null)
            playerRb = playerRoot.GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        controls.Enable();
        controls.OnFoot.Sprint.performed += OnRun;
        controls.OnFoot.Jump.performed += OnJump;
    }

    private void OnDisable()
    {
        controls.OnFoot.Sprint.performed -= OnRun;
        controls.OnFoot.Jump.performed -= OnJump;
        controls.Disable();
    }

    private void OnRun(InputAction.CallbackContext ctx) => TriggerAction();
    private void OnJump(InputAction.CallbackContext ctx) => TriggerAction();
    private void OnInteract(InputAction.CallbackContext ctx) => TriggerAction();

    public void OnTriggered(Collider other, int areaCode)
    {
        if (isResetting) return;
        current_Area = areaCode;
        foreach (var e in npc)
            if (e.area_ID == current_Area)
                e.Activate();
    }

    public void OnExitArea(int areaCode)
    {
        if (current_Area == areaCode)
            current_Area = -1;
    }

    private void TriggerAction()
    {
        if (current_Area == -1 || isResetting) return;
        foreach (var e in npc)
            if (e.area_ID == current_Area)
                e.StartChase(playerRoot);
    }

    public void TriggerOutOfBounds()
    {
        if (current_Area == -1 || isResetting) return;
        foreach (var e in npc)
            if (e.area_ID == current_Area)
                e.StartChase(playerRoot);
    }

    [System.Obsolete]
    public void ResetPlayerAndNPCs()
    {
        if (playerRoot == null || playerStartPoint == null) return;

        isResetting = true;
        current_Area = -1;

        if (playerRb != null)
        {
            playerRb.velocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }

        playerRoot.position = playerStartPoint.position;
        playerRoot.rotation = playerStartPoint.rotation;

        foreach (var e in npc)
            e.ResetNPC();

        StartCoroutine(ReenablePhysicsNextFrame());
    }

    private IEnumerator ReenablePhysicsNextFrame()
    {
        yield return null;
        if (playerRb != null)
            playerRb.isKinematic = false;
        isResetting = false;
    }
}