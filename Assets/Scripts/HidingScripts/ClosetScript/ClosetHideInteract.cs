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

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip turnOnSound;
    [SerializeField] private AudioClip turnOffSound;
    [SerializeField] private AudioClip flickerSound; // <-- Assign your flicker SFX here

    [SerializeField] private AudioClip closetOpenSound;
    [Tooltip("Delay in seconds before the open sound plays.")]
    [SerializeField] private float openSoundDelay = 0f;

    [SerializeField] private AudioClip closetCloseSound;
    [Tooltip("Delay in seconds before the close sound plays.")]
    [SerializeField] private float closeSoundDelay = 0f;

    // --- Aggro Entity References ---
    private AggroEntityDetector aggroEntity;
    private AggroEntityAI aggroEntityAi;
    private AggroEntityWondering aggroEntityWondering;

    // --- NEW: Despawning Entity References ---
    private DespawningEntityDetector despawningEntity;
    private AggroEntityAI despawningEntityAi;
    private DespawningEntityWondering despawningEntityWondering;

    // --- White Lady Reference ---
    private WhiteLady whiteLady;

    public float inputDelay = 2f;
    private bool inputLocked = false;

    private ClosetHidingSystem currentCloset;
    private Interactable closetInteractable;
    private PlayerInteraction playerInteractionScript;
    private Transform playerTransform;

    private bool isTransitioningToHide = false;
    private bool exitUIShown = false;
    private bool isFlickering = false;

    public bool IsHiding => (currentCloset != null && currentCloset.InsideCloset) || isTransitioningToHide;

    void Start()
    {
        currentCloset = GetComponent<ClosetHidingSystem>();
        closetInteractable = GetComponent<Interactable>();

        if (internalClosetLight != null) internalClosetLight.enabled = false;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

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
        if (aggroEntity == null)
        {
            aggroEntity = Object.FindFirstObjectByType<AggroEntityDetector>();
            if (aggroEntity != null)
            {
                aggroEntityAi = aggroEntity.GetComponent<AggroEntityAI>();
                aggroEntityWondering = aggroEntity.GetComponent<AggroEntityWondering>();
            }
        }

        if (despawningEntity == null)
        {
            despawningEntity = Object.FindFirstObjectByType<DespawningEntityDetector>();
            if (despawningEntity != null)
            {
                despawningEntityAi = despawningEntity.GetComponent<AggroEntityAI>();
                despawningEntityWondering = despawningEntity.GetComponent<DespawningEntityWondering>();
            }
        }

        if (whiteLady == null)
        {
            whiteLady = Object.FindFirstObjectByType<WhiteLady>();
        }
    }

    void Update()
    {
        if (inputLocked) return;

        if (aggroEntity == null || despawningEntity == null || whiteLady == null)
        {
            FindEntityReferences();
        }

        if (currentCloset != null && currentCloset.InsideCloset)
        {
            if (!exitUIShown)
            {
                HUDInteractController hud = GetHUD();
                if (hud != null)
                {
                    hud.EnableInteractionText(exitButtonText, exitObjectName, exitActionName);
                }
                exitUIShown = true;
            }

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && !isFlickering)
            {
                if (internalClosetLight != null)
                {
                    internalClosetLight.enabled = !internalClosetLight.enabled;

                    if (internalClosetLight.enabled) PlaySound(turnOnSound);
                    else PlaySound(turnOffSound);
                }
            }

            if (Keyboard.current.gKey.wasPressedThisFrame)
            {
                if (internalClosetLight != null)
                {
                    if (internalClosetLight.enabled) PlaySound(turnOffSound);
                    internalClosetLight.enabled = false;
                }

                exitUIShown = false;
                isTransitioningToHide = false;
                CanInteract = true;

                HUDInteractController hud = GetHUD();
                if (hud != null) hud.DisableInteractionText();

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

        if (aggroEntity != null && !aggroEntity.canHideFromEnemy) canHide = false;
        if (despawningEntity != null && !despawningEntity.canHideFromEnemy) canHide = false;

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
            Debug.Log("An enemy is blocking your ability to hide right now.");
            return;
        }

        if (aggroEntity != null)
        {
            if (aggroEntityAi != null) aggroEntityAi.enabled = false;
            if (aggroEntityWondering != null) aggroEntityWondering.enabled = true;
        }

        if (despawningEntity != null)
        {
            if (despawningEntityAi != null) despawningEntityAi.enabled = false;
            if (despawningEntityWondering != null) despawningEntityWondering.enabled = true;
        }

        if (closetInteractable != null) closetInteractable.DisableOutline();
        if (playerInteractionScript != null) playerInteractionScript.enabled = false;

        HUDInteractController hud = GetHUD();
        if (hud != null) hud.DisableInteractionText();

        CanInteract = false;
        isTransitioningToHide = true;
        exitUIShown = false;

        if (internalClosetLight != null) internalClosetLight.enabled = false;

        PlaySound(closetOpenSound, openSoundDelay);

        StartCoroutine(InputDelay());
        StartCoroutine(currentCloset.GoInsideCloset_CO());
    }

    private IEnumerator ExitClosetRoutine()
    {
        using var scope = new Unity.Profiling.ProfilerMarker("ExitClosetRoutine").Auto();
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
            if (internalClosetLight.enabled) PlaySound(turnOffSound);
            internalClosetLight.enabled = false;
        }

        exitUIShown = false;
        isTransitioningToHide = false;
        CanInteract = true;

        HUDInteractController hud = GetHUD();
        if (hud != null) hud.DisableInteractionText();

        PlaySound(closetCloseSound, closeSoundDelay);

        StartCoroutine(InputDelay());
        StartCoroutine(ExitClosetRoutine());
    }

    public void TriggerClosetLightFlicker(float duration = 1.5f)
    {
        if (currentCloset != null && currentCloset.InsideCloset && internalClosetLight != null && internalClosetLight.enabled && !isFlickering)
        {
            StartCoroutine(FlickerRoutine(duration));
        }
    }

    private IEnumerator FlickerRoutine(float duration)
    {
        isFlickering = true;
        bool originalState = internalClosetLight.enabled;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // --- FIXED: Explicitly flips the state every frame so the audio click perfectly matches a visual blink ---
            internalClosetLight.enabled = !internalClosetLight.enabled;

            PlaySound(flickerSound);

            float waitTime = Random.Range(0.05f, 0.15f);
            yield return new WaitForSeconds(waitTime);
            elapsed += waitTime;
        }

        internalClosetLight.enabled = originalState;
        isFlickering = false;
    }

    private void PlaySound(AudioClip clip, float delay = 0f)
    {
        if (audioSource != null && clip != null)
        {
            if (delay > 0f) StartCoroutine(PlaySoundCO(clip, delay));
            else audioSource.PlayOneShot(clip);
        }
    }

    private IEnumerator PlaySoundCO(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}