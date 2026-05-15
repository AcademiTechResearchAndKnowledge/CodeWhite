using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(ClosetHidingSystem))]
[RequireComponent(typeof(Interactable))]
public class ClosetHideInteract : MonoBehaviour
{
    public bool CanInteract = true;

    [Header("White Lady Settings")]
    public float safeHideDistance = 12f;

    [Header("Exit UI Prompts")]
    [Tooltip("What the HUD should display when you are inside looking to exit.")]
    public string exitButtonText = "G";
    public string exitObjectName = "Closet";
    public string exitActionName = "Exit";

    // --- Old AI References ---
    private EntityDetector entity;
    private EntityAi entityAi;
    private EntityWondering entityWondering;

    // --- New AI Reference ---
    private WhiteLady whiteLady;

    public float inputDelay = 2f;
    private bool inputLocked = false;

    private ClosetHidingSystem currentCloset;
    private Interactable closetInteractable;
    private PlayerInteraction playerInteractionScript;
    private Transform playerTransform;

    private bool isTransitioningToHide = false;
    private bool exitUIShown = false;

    public bool IsHiding => (currentCloset != null && currentCloset.InsideCloset) || isTransitioningToHide;

    void Start()
    {
        currentCloset = GetComponent<ClosetHidingSystem>();
        closetInteractable = GetComponent<Interactable>();

        // Cache player references
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerInteractionScript = playerObj.GetComponent<PlayerInteraction>();
        }

        FindEntityReferences();
    }

    void FindEntityReferences()
    {
        entity = Object.FindFirstObjectByType<EntityDetector>();
        if (entity != null)
        {
            entityAi = entity.GetComponent<EntityAi>();
            entityWondering = entity.GetComponent<EntityWondering>();
        }

        whiteLady = Object.FindFirstObjectByType<WhiteLady>();
    }

    void Update()
    {
        if (inputLocked) return;

        if (entity == null && whiteLady == null)
        {
            FindEntityReferences();
        }

        // --- WHILE INSIDE THE CLOSET ---
        if (currentCloset != null && currentCloset.InsideCloset)
        {
            // 1. Push the Exit prompt to the HUD once fully inside
            if (!exitUIShown)
            {
                HUDInteractController hud = GetHUD();
                if (hud != null)
                {
                    hud.EnableInteractionText(exitButtonText, exitObjectName, exitActionName);
                }
                exitUIShown = true;
            }

            // 2. Listen for the Exit Key (G) directly
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                exitUIShown = false;
                isTransitioningToHide = false;
                CanInteract = true;

                // Clear the HUD instantly
                HUDInteractController hud = GetHUD();
                if (hud != null) hud.DisableInteractionText();

                StartCoroutine(InputDelay());
                StartCoroutine(ExitClosetRoutine());
            }
        }
    }

    // --- TRIGGERED BY YOUR INTERACTABLE SCRIPT'S UNITY EVENT ---
    public void TryEnterCloset()
    {
        if (!CanInteract || inputLocked || (currentCloset != null && currentCloset.InsideCloset)) return;

        // Fallback reference check
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
                playerInteractionScript = playerObj.GetComponent<PlayerInteraction>();
            }
        }

        bool canHide = true;

        if (entity != null && !entity.canHideFromEnemy)
        {
            canHide = false;
        }

        if (whiteLady != null && playerTransform != null)
        {
            float distanceToWL = Vector3.Distance(playerTransform.position, whiteLady.transform.position);

            if (whiteLady.CurrentState == WhiteLady.State.Chasing && distanceToWL < safeHideDistance)
            {
                canHide = false;
            }
        }

        if (!canHide)
        {
            Debug.Log("Enemy is too close. Cannot hide yet.");
            return;
        }

        // Apply AI overrides
        if (entity != null)
        {
            if (entityAi != null) entityAi.enabled = false;
            if (entityWondering != null) entityWondering.enabled = true;
        }

        // 1. Disable the outline immediately
        if (closetInteractable != null) closetInteractable.DisableOutline();

        // 2. Disable the PlayerInteraction script temporarily so it doesn't fight over the HUD while you hide
        if (playerInteractionScript != null) playerInteractionScript.enabled = false;

        // 3. Clear the initial "Hide" prompt from the screen
        HUDInteractController hud = GetHUD();
        if (hud != null) hud.DisableInteractionText();

        CanInteract = false;
        isTransitioningToHide = true;
        exitUIShown = false;

        StartCoroutine(InputDelay());
        StartCoroutine(currentCloset.GoInsideCloset_CO());
    }

    private IEnumerator ExitClosetRoutine()
    {
        // Wait for your existing exit transition to finish moving the player
        yield return StartCoroutine(currentCloset.GoOutsideCloset_CO());

        // Once fully outside, turn the player's standard raycaster back on
        if (playerInteractionScript != null)
        {
            playerInteractionScript.enabled = true;
        }
    }

    IEnumerator InputDelay()
    {
        inputLocked = true;
        yield return new WaitForSeconds(inputDelay);
        inputLocked = false;
    }

    // Safely fetches your HUD singleton matching your PlayerInteraction style
    HUDInteractController GetHUD()
    {
        if (HUDInteractController.Instance != null) return HUDInteractController.Instance;

        HUDInteractController found = Object.FindFirstObjectByType<HUDInteractController>();
        if (found != null)
        {
            HUDInteractController.Instance = found;
            return found;
        }
        return null;
    }

    // --- STALKER FORCED EXIT CLEANUP ---
    public void ForceKickedOutByStalker()
    {
        exitUIShown = false;
        isTransitioningToHide = false;
        CanInteract = true;

        // Clear the HUD instantly
        HUDInteractController hud = GetHUD();
        if (hud != null) hud.DisableInteractionText();

        StartCoroutine(InputDelay());
        StartCoroutine(ExitClosetRoutine());
    }
}