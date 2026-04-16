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
    public PlayerReferences player; // Reference to freeze the player during the intro
    public Outline itemOutline;
    public Outline doorOutline;

    private TutorialState currentState = TutorialState.Walk;
    private bool isFading = false;

    void Start()
    {
        // Ensure outlines are off at the start
        if (itemOutline != null) itemOutline.enabled = false;
        if (doorOutline != null) doorOutline.enabled = false;

        textCanvasGroup.alpha = 0;

        // Start the cinematic intro!
        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.alpha = 1; // Make sure it starts pitch black
            StartCoroutine(IntroSequence());
        }
        else
        {
            // Fallback if you forget to assign the black screen
            StartCoroutine(ShowMessage("WASD to walk"));
        }
    }

    void Update()
    {
        if (isFading) return; // Don't check inputs while transitioning

        // Ensure devices exist before checking to prevent errors
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
                    if (itemOutline != null) itemOutline.enabled = true;
                    AdvanceTutorial(TutorialState.Crouch, "Press Left Ctrl to crouch");
                }
                break;

            case TutorialState.Crouch:
                if (hasKeyboard && Keyboard.current.leftCtrlKey.wasPressedThisFrame)
                {
                    AdvanceTutorial(TutorialState.Pickup, "Press F to pick-up the item");
                }
                break;

            // Pickup and UseItem are handled by external scripts calling public methods below

            case TutorialState.Sprint:
                if (hasKeyboard && Keyboard.current.leftShiftKey.wasPressedThisFrame)
                {
                    AdvanceTutorial(TutorialState.Portal, "Go through the portal");
                }
                break;

                // Portal is handled by an external trigger calling a public method below
        }
    }

    private void AdvanceTutorial(TutorialState nextState, string nextMessage)
    {
        currentState = nextState;
        StartCoroutine(TransitionMessage(nextMessage));
    }

    // --- PUBLIC METHODS FOR EXTERNAL SCRIPTS ---

    public void ItemPickedUp()
    {
        if (currentState == TutorialState.Pickup && !isFading)
        {
            if (itemOutline != null) itemOutline.enabled = false;
            AdvanceTutorial(TutorialState.UseItem, "Use scroll wheel or numbers to select the item and press E to use");
        }
    }

    public void ItemUsed()
    {
        if (currentState == TutorialState.UseItem && !isFading)
        {
            if (doorOutline != null) doorOutline.enabled = true;
            AdvanceTutorial(TutorialState.Door, "Go to the door and press F to interact");
        }
    }

    public void DoorInteracted()
    {
        if (currentState == TutorialState.Door && !isFading)
        {
            if (doorOutline != null) doorOutline.enabled = false;
            AdvanceTutorial(TutorialState.Sprint, "Press Shift to sprint");
        }
    }

    public void PortalEntered()
    {
        if (currentState == TutorialState.Portal && !isFading)
        {
            currentState = TutorialState.Finished;
            StartCoroutine(HideMessage());
        }
    }

    // --- HELPER METHODS ---

    private void TogglePlayerControls(bool state)
    {
        if (player == null) return;

        if (player.movementScript != null) player.movementScript.enabled = state;
        if (player.flashlightScript != null) player.flashlightScript.enabled = state;

        // Disable the camera script so they can't look around in the dark
        if (player.playerCam != null) player.playerCam.enabled = state;
    }

    // --- COROUTINES FOR SEQUENCES AND UI ---

    private IEnumerator IntroSequence()
    {
        isFading = true;

        // Disable controls before the fade starts
        TogglePlayerControls(false);

        // Wait a moment in the dark
        yield return new WaitForSeconds(startDelay);

        // Fade the black screen away
        float timer = 0;
        while (timer < blackScreenFadeDuration)
        {
            timer += Time.deltaTime;
            blackScreenCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / blackScreenFadeDuration);
            yield return null;
        }

        blackScreenCanvasGroup.alpha = 0;
        blackScreenCanvasGroup.gameObject.SetActive(false); // Turn off so it doesn't block UI clicks

        // Re-enable controls once the fade is done
        TogglePlayerControls(true);

        isFading = false;
        yield return StartCoroutine(ShowMessage("WASD to walk"));
    }

    private IEnumerator ShowMessage(string message)
    {
        isFading = true;
        tutorialText.text = message;

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        textCanvasGroup.alpha = 1;
        isFading = false;
    }

    private IEnumerator HideMessage()
    {
        isFading = true;

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        textCanvasGroup.alpha = 0;
        isFading = false;
    }

    private IEnumerator TransitionMessage(string nextMessage)
    {
        yield return StartCoroutine(HideMessage());
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(ShowMessage(nextMessage));
    }
}