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

    [Header("Closet Internal Light")]
    [Tooltip("Assign the light component placed inside the closet here.")]
    public Light internalClosetLight;

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

    // Added a flag to prevent toggling while it's currently flickering
    private bool isFlickering = false;

    public bool IsHiding => (currentCloset != null && currentCloset.InsideCloset) || isTransitioningToHide;

    void Start()
    {
        currentCloset = GetComponent<ClosetHidingSystem>();
        closetInteractable = GetComponent<Interactable>();

        // Ensure the inside closet light is off when the game starts
        if (internalClosetLight != null) internalClosetLight.enabled = false;

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

            // 2. Listen for Right-Click to toggle the internal closet light (only if not currently flickering)
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && !isFlickering)
            {
                if (internalClosetLight != null)
                {
                    internalClosetLight.enabled = !internalClosetLight.enabled;
                }
            }

            // 3. Listen for the Exit Key (G) directly
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                if (internalClosetLight != null) internalClosetLight.enabled = false;

                exitUIShown = false;
                isTransitioningToHide = false;
                CanInteract = true;

                HUDInteractController hud = GetHUD();
                if (hud != null) hud.DisableInteractionText();

                StartCoroutine(InputDelay());
                StartCoroutine(ExitClosetRoutine());
            }
        }
    }

    public void TryEnterCloset()
    {
        if (!CanInteract || inputLocked || (currentCloset != null && currentCloset.InsideCloset)) return;

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

        if (entity != null && !entity.canHideFromEnemy) canHide = false;

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

        if (entity != null)
        {
            if (entityAi != null) entityAi.enabled = false;
            if (entityWondering != null) entityWondering.enabled = true;
        }

        if (closetInteractable != null) closetInteractable.DisableOutline();
        if (playerInteractionScript != null) playerInteractionScript.enabled = false;

        HUDInteractController hud = GetHUD();
        if (hud != null) hud.DisableInteractionText();

        CanInteract = false;
        isTransitioningToHide = true;
        exitUIShown = false;

        if (internalClosetLight != null) internalClosetLight.enabled = false;

        StartCoroutine(InputDelay());
        StartCoroutine(currentCloset.GoInsideCloset_CO());
    }

    private IEnumerator ExitClosetRoutine()
    {
        yield return StartCoroutine(currentCloset.GoOutsideCloset_CO());
        if (playerInteractionScript != null) playerInteractionScript.enabled = true;
    }

    IEnumerator InputDelay()
    {
        inputLocked = true;
        yield return new WaitForSeconds(inputDelay);
        inputLocked = false;
    }

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

    public void ForceKickedOutByStalker()
    {
        if (internalClosetLight != null) internalClosetLight.enabled = false;

        exitUIShown = false;
        isTransitioningToHide = false;
        CanInteract = true;

        HUDInteractController hud = GetHUD();
        if (hud != null) hud.DisableInteractionText();

        StartCoroutine(InputDelay());
        StartCoroutine(ExitClosetRoutine());
    }

    // „Ÿ„Ÿ„Ÿ NEW: FLICKER LOGIC „Ÿ„Ÿ„Ÿ
    public void TriggerClosetLightFlicker(float duration = 1.5f)
    {
        // Only flicker if we are actually hiding inside THIS closet
        if (currentCloset != null && currentCloset.InsideCloset && internalClosetLight != null && !isFlickering)
        {
            StartCoroutine(FlickerRoutine(duration));
        }
    }

    private IEnumerator FlickerRoutine(float duration)
    {
        isFlickering = true;
        bool originalState = internalClosetLight.enabled; // Remember if they had it on or off
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Randomly toggle the light
            internalClosetLight.enabled = Random.value > 0.5f;

            // Random wait time between flashes (fast flicker)
            float waitTime = Random.Range(0.05f, 0.15f);
            yield return new WaitForSeconds(waitTime);
            elapsed += waitTime;
        }

        // Restore to however they had it before the White Lady teleported
        internalClosetLight.enabled = originalState;
        isFlickering = false;
    }
}