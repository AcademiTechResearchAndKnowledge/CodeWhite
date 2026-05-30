using System.Collections;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public enum TutorialState
    {
        Intro,
        Explore_Start,       // Waiting to hit the first barrier
        Door_Objective,      // Objective: Go to the door
        Portal_Objective,    // Objective: Go through the portal
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
    [Tooltip("Can be left blank in Inspector; will find automatically at runtime.")]
    public PlayerReferences player;

    private TutorialState currentState = TutorialState.Intro;
    private Coroutine activeUICoroutine;
    public RandomPortalSpawner RPS;

    private void Awake()
    {
        Instance = this;

        // NEW: Automatically find the player in the current scene
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerReferences>();

            if (player == null)
            {
                Debug.LogError("TutorialManager: Could not find a PlayerReferences component in the scene!");
            }
        }

        if (blackScreenCanvasGroup != null)
        {
            blackScreenCanvasGroup.gameObject.SetActive(true);
            blackScreenCanvasGroup.alpha = 1;

            // This will now work properly because the player reference was found above
            ToggleAllPlayerControls(false);
        }

        if (RPS == null)
            RPS = FindFirstObjectByType<RandomPortalSpawner>();
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
            UnlockPlayerControls();
            currentState = TutorialState.Explore_Start;
        }
    }

    public void HandleBarrierTriggered(string triggerID, string dialogueID)
    {
        if (triggerID == "StartBarrier" && currentState == TutorialState.Explore_Start)
        {
            PlayDialogueAndAdvance(dialogueID, TutorialState.Door_Objective, "Go to the door");
        }
        else if (triggerID == "DoorBarrier" && currentState == TutorialState.Door_Objective)
        {
            PlayDialogueAndAdvance(dialogueID, TutorialState.Portal_Objective, "Go through the portal");
            RPS.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Horizontal);
        }
        else if (triggerID == "PortalBarrier" && currentState == TutorialState.Portal_Objective)
        {
            currentState = TutorialState.Finished;
            if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
            activeUICoroutine = StartCoroutine(HideMessage());
        }
    }

    private void PlayDialogueAndAdvance(string dialogueID, TutorialState nextState, string nextMessage)
    {
        if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
        activeUICoroutine = StartCoroutine(HideMessage());

        var data = DialogueDatabase.Instance.GetDialogue(dialogueID);

        ToggleAllPlayerControls(false);

        DialogueManager.Instance.StartDialogue(data, false, () =>
        {
            UnlockPlayerControls();

            currentState = nextState;
            if (activeUICoroutine != null) StopCoroutine(activeUICoroutine);
            activeUICoroutine = StartCoroutine(ShowMessage(nextMessage));
        });
    }

    private void ToggleAllPlayerControls(bool state)
    {
        if (player == null) return;

        if (player.movementScript != null) player.movementScript.enabled = state;
        if (player.flashlightScript != null) player.flashlightScript.enabled = state;
        if (player.playerLook != null) player.playerLook.canLook = state;
    }

    private void UnlockPlayerControls()
    {
        if (player == null) return;

        if (player.movementScript != null)
        {
            player.movementScript.enabled = true;
            player.movementScript.canCrouch = true;
            player.movementScript.canSprint = true;
        }
        if (player.playerLook != null) player.playerLook.canLook = true;
        if (player.flashlightScript != null) player.flashlightScript.enabled = true;
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

        UnlockPlayerControls();
        currentState = TutorialState.Explore_Start;
    }

    public IEnumerator FadeBlackScreenInOut()
    {
        ToggleAllPlayerControls(false);
        blackScreenCanvasGroup.gameObject.SetActive(true);

        float timer = 0;
        while (timer < blackScreenFadeDuration)
        {
            timer += Time.deltaTime;
            blackScreenCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / blackScreenFadeDuration);
            yield return null;
        }

        yield return new WaitForSeconds(1f); // Hold black screen

        timer = 0;
        while (timer < blackScreenFadeDuration)
        {
            timer += Time.deltaTime;
            blackScreenCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / blackScreenFadeDuration);
            yield return null;
        }

        blackScreenCanvasGroup.gameObject.SetActive(false);
        UnlockPlayerControls();
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