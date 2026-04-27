using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState
    {
        Walk,
        Flashlight,
        Crouch,
        Pickup,
        UseItem,
        Door,
        Sprint,
        Portal,
        Finished
    }

    [Header("UI References")]
    public TextMeshProUGUI tutorialText;
    public CanvasGroup textCanvasGroup;
    public float fadeDuration = 0.5f;

    [Header("Cinematic Settings")]
    public CanvasGroup blackScreenCanvasGroup;
    public float blackScreenFadeDuration = 2.0f;
    public float startDelay = 1.0f;

    [Header("Game Object References")]
    public PlayerReferences player;
    public Outline itemOutline;
    public Outline doorOutline;

    private TutorialState currentState = TutorialState.Walk;

    // Tracks the current UI animation so we can interrupt it if the player is fast
    private Coroutine activeUICoroutine;

    void Start()
    {
        // Ensure outlines are off at the start
        if (itemOutline != null) itemOutline.enabled = false;
        if (doorOutline != null) doorOutline.enabled = false;

        textCanvasGroup.alpha = 0;

        // Start the cinematic intro!
        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.alpha = 1;
            StartCoroutine(IntroSequence());
        }
        else
        {
            LockAdvancedControls();

            if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
            activeUICoroutine = StartCoroutine(ShowMessage("WASD to walk"));
        }
    }

    void Update()
    {
        bool hasKeyboard = Keyboard.current != null;
        bool hasMouse = Mouse.current != null;

        switch (currentState)
        {
            case TutorialState.Walk:
                if (hasKeyboard && (Keyboard.current.wKey.wasPressedThisFrame ||
                                    Keyboard.current.aKey.wasPressedThisFrame ||
                                    Keyboard.current.sKey.wasPressedThisFrame ||
                                    Keyboard.current.dKey.wasPressedThisFrame))
                {
                    AdvanceTutorial(TutorialState.Flashlight, "Right click to turn on flashlight");
                }
                break;

            case TutorialState.Flashlight:
                if (hasMouse && Mouse.current.rightButton.wasPressedThisFrame)
                {
                    // Removed the item outline trigger from here
                    AdvanceTutorial(TutorialState.Crouch, "Press Left Ctrl to crouch");
                }
                break;

            case TutorialState.Crouch:
                if (hasKeyboard && Keyboard.current.leftCtrlKey.wasPressedThisFrame)
                {
                    // Added the item outline trigger here, right as the pickup step starts!
                    if (itemOutline != null) itemOutline.enabled = true;
                    AdvanceTutorial(TutorialState.Pickup, "Press F to pick-up the item");
                }
                break;

            case TutorialState.Sprint:
                if (hasKeyboard && Keyboard.current.leftShiftKey.wasPressedThisFrame)
                {
                    AdvanceTutorial(TutorialState.Portal, "Go through the portal");
                }
                break;
        }
    }

    private void AdvanceTutorial(TutorialState nextState, string nextMessage)
    {
        currentState = nextState;

        UnlockMechanic(nextState);

        // Stop any currently playing text fades and start the new one
        if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
        activeUICoroutine = StartCoroutine(TransitionMessage(nextMessage));
    }

    // --- PUBLIC METHODS FOR EXTERNAL SCRIPTS ---

    public void ItemPickedUp()
    {
        if (currentState == TutorialState.Pickup)
        {
            if (itemOutline != null) itemOutline.enabled = false;
            AdvanceTutorial(TutorialState.UseItem, "Use scroll wheel or numbers to select the item and press E to use");
        }
    }

    public void ItemUsed()
    {
        if (currentState == TutorialState.UseItem)
        {
            if (doorOutline != null) doorOutline.enabled = true;
            AdvanceTutorial(TutorialState.Door, "Go to the door and press F to interact");
        }
    }

    public void DoorInteracted()
    {
        if (currentState == TutorialState.Door)
        {
            if (doorOutline != null) doorOutline.enabled = false;
            AdvanceTutorial(TutorialState.Sprint, "Press Shift to sprint");
        }
    }

    public void PortalEntered()
    {
        if (currentState == TutorialState.Portal)
        {
            currentState = TutorialState.Finished;

            if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
            activeUICoroutine = StartCoroutine(HideMessage());
        }
    }

    // --- CONTROL LOCKING / UNLOCKING ---

    private void ToggleAllPlayerControls(bool state)
    {
        if (player == null) return;
        if (player.movementScript != null) player.movementScript.enabled = state;
        if (player.flashlightScript != null) player.flashlightScript.enabled = state;

        // Target the playerLook script instead of playerCam
        if (player.playerLook != null) player.playerLook.canLook = state;
    }

    private void LockAdvancedControls()
    {
        if (player == null) return;

        if (player.movementScript != null) player.movementScript.enabled = true;

        // Target the playerLook script instead of playerCam
        if (player.playerLook != null) player.playerLook.canLook = true;

        if (player.flashlightScript != null) player.flashlightScript.enabled = false;

        if (player.movementScript != null)
        {
            player.movementScript.canCrouch = false;
            player.movementScript.canSprint = false;
        }
    }

    private void UnlockMechanic(TutorialState state)
    {
        if (player == null) return;

        switch (state)
        {
            case TutorialState.Flashlight:
                if (player.flashlightScript != null) player.flashlightScript.enabled = true;
                break;
            case TutorialState.Crouch:
                if (player.movementScript != null) player.movementScript.canCrouch = true;
                break;
            case TutorialState.Sprint:
                if (player.movementScript != null) player.movementScript.canSprint = true;
                break;
        }
    }

    // --- COROUTINES FOR SEQUENCES AND UI ---

    private IEnumerator IntroSequence()
    {
        ToggleAllPlayerControls(false);

        yield return new WaitForSeconds(startDelay);

        float timer = 0;
        while (timer < blackScreenFadeDuration)
        {
            timer += Time.deltaTime;
            blackScreenCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / blackScreenFadeDuration);
            yield return null;
        }

        blackScreenCanvasGroup.alpha = 0;
        blackScreenCanvasGroup.gameObject.SetActive(false);

        LockAdvancedControls();

        if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
        activeUICoroutine = StartCoroutine(ShowMessage("WASD to walk"));
    }

    private IEnumerator ShowMessage(string message)
    {
        tutorialText.text = message;

        float timer = 0;
        float startAlpha = textCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1, timer / fadeDuration);
            yield return null;
        }

        textCanvasGroup.alpha = 1;
    }

    private IEnumerator HideMessage()
    {
        float timer = 0;
        float startAlpha = textCanvasGroup.alpha;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, timer / fadeDuration);
            yield return null;
        }

        textCanvasGroup.alpha = 0;
    }

    private IEnumerator TransitionMessage(string nextMessage)
    {
        yield return StartCoroutine(HideMessage());
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShowMessage(nextMessage));
    }
}