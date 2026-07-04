using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class IntroSkipper : MonoBehaviour
{
    [Header("Scene Transition")]
    [Tooltip("The exact name of your main menu scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI Fade Settings")]
    [Tooltip("The Canvas Group attached to your 'Press Space' text")]
    [SerializeField] private CanvasGroup skipPromptGroup;
    [SerializeField] private float delayBeforeShow = 2.0f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float displayDuration = 3.0f;
    [SerializeField] private float fadeOutDuration = 1.0f;

    private void Start()
    {
        // Make sure the prompt is completely invisible when the scene starts
        if (skipPromptGroup != null)
        {
            skipPromptGroup.alpha = 0f;
            StartCoroutine(ShowPromptSequence());
        }
        else
        {
            Debug.LogWarning("Skip Prompt CanvasGroup is not assigned!");
        }
    }

    private void Update()
    {
        // Safety check to ensure a keyboard is actually connected
        if (Keyboard.current == null) return;

        // The New Input System equivalent of GetKeyDown
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            SkipIntro();
        }
    }

    private IEnumerator ShowPromptSequence()
    {
        // 1. Wait before fading in
        yield return new WaitForSeconds(delayBeforeShow);

        // 2. Fade in smoothly
        float time = 0f;
        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            skipPromptGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeInDuration);
            yield return null; // Wait for the next frame
        }
        skipPromptGroup.alpha = 1f;

        // 3. Keep it visible for a few seconds
        yield return new WaitForSeconds(displayDuration);

        // 4. Fade out smoothly
        time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            skipPromptGroup.alpha = Mathf.Lerp(1f, 0f, time / fadeOutDuration);
            yield return null;
        }
        skipPromptGroup.alpha = 0f;
    }

    private void SkipIntro()
    {
        // Stop the fading coroutine immediately if they press space
        StopAllCoroutines();
        SceneManager.LoadScene(mainMenuSceneName);
    }
}