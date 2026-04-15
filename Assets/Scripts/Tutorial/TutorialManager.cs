using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Make sure to add this namespace!
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

    [Header("Game Object References")]
    public Outline itemOutline;
    public Outline doorOutline;

    private TutorialState currentState = TutorialState.Walk;
    private bool isFading = false;

    void Start()
    {
        if (itemOutline != null) itemOutline.enabled = false;
        if (doorOutline != null) doorOutline.enabled = false;

        textCanvasGroup.alpha = 0;
        StartCoroutine(ShowMessage("WASD to walk"));
    }

    void Update()
    {
        if (isFading) return;

        // It is good practice in the new system to ensure the device exists 
        // before checking it, preventing errors if a keyboard/mouse isn't plugged in.
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
                // rightButton is the equivalent of GetMouseButtonDown(1)
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

    // --- COROUTINES FOR FADING UI ---

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