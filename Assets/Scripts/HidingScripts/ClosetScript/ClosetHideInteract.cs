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

    // --- ADDED: Audio Settings ---
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip turnOnSound;
    [SerializeField] private AudioClip turnOffSound;

    [SerializeField] private AudioClip closetOpenSound;
    [Tooltip("Delay in seconds before the open sound plays.")]
    [SerializeField] private float openSoundDelay = 0f;

    [SerializeField] private AudioClip closetCloseSound;
    [Tooltip("Delay in seconds before the close sound plays.")]
    [SerializeField] private float closeSoundDelay = 0f;

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

        // Auto-grab AudioSource if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

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

                    // Play toggle sounds
                    if (internalClosetLight.enabled)
                    {
                        PlaySound(turnOnSound);
                    }
                    else
                    {
                        PlaySound(turnOffSound);
                    }
                }
            }

            // 3. Listen for the Exit Key (G) directly
            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                if (internalClosetLight != null)
                {
                    // Only play the off sound if it was actually on when we exited
                    if (internalClosetLight.enabled)
                    {
                        PlaySound(turnOffSound);
                    }
                    internalClosetLight.enabled = false;
                }

                exitUIShown = false;
                isTransitioningToHide = false;
                CanInteract = true;

                HUDInteractController hud = GetHUD();
                if (hud != null) hud.DisableInteractionText();

                // Play close sound when exiting manually with inspector delay
                PlaySound(closetCloseSound, closeSoundDelay);

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

        // Play open sound when entering the closet with inspector delay
        PlaySound(closetOpenSound, openSoundDelay);

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
        if (internalClosetLight != null)
        {
            // Play the off sound if the stalker kicks us out while the light is on
            if (internalClosetLight.enabled)
            {
                PlaySound(turnOffSound);
            }
            internalClosetLight.enabled = false;
        }

        exitUIShown = false;
        isTransitioningToHide = false;
        CanInteract = true;

        HUDInteractController hud = GetHUD();
        if (hud != null) hud.DisableInteractionText();

        // Play close sound when forced out by an enemy with inspector delay
        PlaySound(closetCloseSound, closeSoundDelay);

        StartCoroutine(InputDelay());
        StartCoroutine(ExitClosetRoutine());
    }

    // „Ÿ„Ÿ„Ÿ FLICKER LOGIC „Ÿ„Ÿ„Ÿ
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

    // --- Helper method to safely play audio with optional delay ---
    private void PlaySound(AudioClip clip, float delay = 0f)
    {
        if (audioSource != null && clip != null)
        {
            if (delay > 0f)
            {
                StartCoroutine(PlaySoundCO(clip, delay));
            }
            else
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    // --- Coroutine to handle the actual timing ---
    private IEnumerator PlaySoundCO(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}