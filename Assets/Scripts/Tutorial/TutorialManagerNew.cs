using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TutorialManagerNew : MonoBehaviour
{
    public static TutorialManagerNew Instance;

    public enum TutorialState
    {
        Intro,
        Walk_Text,           // Showing WASD
        Walk_Explore,        // Waiting to hit the Dark Barrier
        Flashlight_Text,     // Showing Flashlight text
        Flashlight_Explore,  // Waiting to hit the Tree Barrier
        Crouch_Text,         // Showing Crouch text
        Crouch_Explore,      // Waiting to hit Scream Barrier
        Sprint_Text,         // Showing Sprint text
        Sprint_Explore,      // NEW: Waiting to hit the "What is that" barrier
        Panic_Explore,       // NEW: Waiting to hit the final abduction/help barrier
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

    private TutorialState currentState = TutorialState.Intro;
    private Coroutine activeUICoroutine;

    private void Awake()
    {
        Instance = this;

        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.gameObject.SetActive(true);
            blackScreenCanvasGroup.alpha = 1;
        }
    }

    void Start()
    {
        textCanvasGroup.alpha = 0;

        if (blackScreenCanvasGroup != null)
        {
            StartCoroutine(IntroSequence());
        }
        else
        {
            LockAdvancedControls();
            currentState = TutorialState.Walk_Text;
            if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
            activeUICoroutine = StartCoroutine(ShowMessage("[WASD] to walk"));
        }
    }

    void Update()
    {
        bool hasKeyboard = Keyboard.current != null;
        bool hasMouse = Mouse.current != null;

        switch (currentState)
        {
            case TutorialState.Walk_Text:
                if (hasKeyboard && (Keyboard.current.wKey.wasPressedThisFrame ||
                                    Keyboard.current.aKey.wasPressedThisFrame ||
                                    Keyboard.current.sKey.wasPressedThisFrame ||
                                    Keyboard.current.dKey.wasPressedThisFrame))
                {
                    currentState = TutorialState.Walk_Explore;
                    if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
                    activeUICoroutine = StartCoroutine(HideMessage());
                }
                break;

            case TutorialState.Flashlight_Text:
                if (hasMouse && Mouse.current.rightButton.wasPressedThisFrame)
                {
                    currentState = TutorialState.Flashlight_Explore;
                    if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
                    activeUICoroutine = StartCoroutine(HideMessage());
                }
                break;

            case TutorialState.Crouch_Text:
                if (hasKeyboard && Keyboard.current.leftCtrlKey.wasPressedThisFrame)
                {
                    currentState = TutorialState.Crouch_Explore;
                    if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
                    activeUICoroutine = StartCoroutine(HideMessage());
                }
                break;

            case TutorialState.Sprint_Text:
                if (hasKeyboard && Keyboard.current.leftShiftKey.wasPressedThisFrame)
                {
                    currentState = TutorialState.Sprint_Explore;
                    if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
                    activeUICoroutine = StartCoroutine(HideMessage());
                }
                break;
        }
    }

    public void HandleBarrierTriggered(string triggerID, string dialogueID)
    {
        if (triggerID == "DarkBarrier" && currentState == TutorialState.Walk_Explore)
        {
            PlayDialogueAndAdvance(dialogueID, TutorialState.Flashlight_Text, "[Right Click] to turn on flashlight");
            UnlockMechanic(TutorialState.Flashlight_Text);
        }
        else if (triggerID == "TreeBarrier" && currentState == TutorialState.Flashlight_Explore)
        {
            PlayDialogueAndAdvance(dialogueID, TutorialState.Crouch_Text, "Press [Left Ctrl] to crouch");
            UnlockMechanic(TutorialState.Crouch_Text);
        }
        else if (triggerID == "ScreamBarrier" && currentState == TutorialState.Crouch_Explore)
        {
            PlayDialogueAndAdvance(dialogueID, TutorialState.Sprint_Text, "Press [Shift] to sprint");
            UnlockMechanic(TutorialState.Sprint_Text);
        }
        else if (triggerID == "WhatIsThatBarrier" && currentState == TutorialState.Sprint_Explore)
        {
            PlayDialogueAndAdvance(dialogueID, TutorialState.Panic_Explore, "");
        }
        else if (triggerID == "AbductionBarrier" && currentState == TutorialState.Panic_Explore)
        {
            PlayDialogueAndAdvance(dialogueID, TutorialState.Finished, "");
        }
    }

    private void PlayDialogueAndAdvance(string dialogueID, TutorialState nextState, string nextMessage)
    {
        if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
        activeUICoroutine = StartCoroutine(HideMessage());

        var data = DialogueDatabase.Instance.GetDialogue(dialogueID);

        DialogueManager.Instance.StartDialogue(data, false, () =>
        {
            currentState = nextState;
            if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);

            if (!string.IsNullOrEmpty(nextMessage))
            {
                activeUICoroutine = StartCoroutine(ShowMessage(nextMessage));
            }
        });
    }

    private void ToggleAllPlayerControls(bool state)
    {
        if (player == null) return;
        if (player.movementScript != null) player.movementScript.enabled = state;
        if (player.flashlightScript != null) player.flashlightScript.enabled = state;
        if (player.playerLook != null) player.playerLook.canLook = state;
    }

    private void LockAdvancedControls()
    {
        if (player == null) return;
        if (player.movementScript != null) player.movementScript.enabled = true;
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
            case TutorialState.Flashlight_Text:
                if (player.flashlightScript != null) player.flashlightScript.enabled = true;
                break;
            case TutorialState.Crouch_Text:
                if (player.movementScript != null) player.movementScript.canCrouch = true;
                break;
            case TutorialState.Sprint_Text:
                if (player.movementScript != null) player.movementScript.canSprint = true;
                break;
        }
    }

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

        currentState = TutorialState.Walk_Text;

        if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
        activeUICoroutine = StartCoroutine(ShowMessage("[WASD] to walk"));
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
}