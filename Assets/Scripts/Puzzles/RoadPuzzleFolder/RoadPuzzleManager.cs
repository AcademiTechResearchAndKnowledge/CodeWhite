using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerActionDetector : MonoBehaviour
{
    private PlayerInput controls;

    [Header("References")]
    public PlayerReferences playerRef;
    public Transform playerStartPoint;
    public EntityAi[] npc;

    private int current_Area = -1;
    private bool isResetting = false;

    private void Awake()
    {
        controls = new PlayerInput();

        // Try to find the player as soon as the scene loads
        FindPlayerReference();
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

    // --- Adapted from your Closet script logic ---
    private void FindPlayerReference()
    {
        // Skip if we already successfully found it
        if (playerRef != null) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("PlayerActionDetector: No Player tagged object found yet.");
            return;
        }

        playerRef = playerObject.GetComponent<PlayerReferences>();

        if (playerRef == null)
        {
            Debug.LogError("PlayerActionDetector: PlayerReferences is missing on the Player tagged object.");
        }
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
        FindPlayerReference(); // Failsafe in case player spawned late

        if (current_Area == -1 || isResetting || playerRef == null) return;

        foreach (var e in npc)
            if (e.area_ID == current_Area)
                e.StartChase(playerRef.transform);
    }

    public void TriggerOutOfBounds()
    {
        FindPlayerReference(); // Failsafe in case player spawned late

        if (current_Area == -1 || isResetting || playerRef == null) return;

        foreach (var e in npc)
            if (e.area_ID == current_Area)
                e.StartChase(playerRef.transform);
    }

    [System.Obsolete]
    public void ResetPlayerAndNPCs()
    {
        FindPlayerReference(); // Failsafe check before resetting

        if (playerRef == null || playerStartPoint == null) return;

        isResetting = true;
        current_Area = -1;

        if (playerRef.rb != null)
        {
            playerRef.rb.velocity = Vector3.zero;
            playerRef.rb.angularVelocity = Vector3.zero;
            playerRef.rb.isKinematic = true;
        }

        playerRef.transform.position = playerStartPoint.position;
        playerRef.transform.rotation = playerStartPoint.rotation;

        foreach (var e in npc)
            e.ResetNPC();

        StartCoroutine(ReenablePhysicsNextFrame());
    }

    private IEnumerator ReenablePhysicsNextFrame()
    {
        yield return null;

        if (playerRef != null && playerRef.rb != null)
            playerRef.rb.isKinematic = false;

        isResetting = false;
    }
}