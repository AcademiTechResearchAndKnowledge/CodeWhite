using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Refs")]
    [SerializeField] private PlayerStats playerStats;

    private bool isDead;
    private bool canCheckDeath;

    private void Awake()
    {
        Time.timeScale = 1f;

        isDead = false;
        canCheckDeath = false;

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        ResetUIState();

        AudioReset();

        StartCoroutine(EnableDeathCheckNextFrame());
    }

    private void OnEnable()
    {
        AudioReset();
    }

    private IEnumerator EnableDeathCheckNextFrame()
    {
        yield return null;
        canCheckDeath = true;
    }

    private void Update()
    {
        if (!canCheckDeath) return;
        if (isDead) return;

        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
            if (playerStats == null) return;
        }

        if (playerStats.Anxiety >= playerStats.MaxAnxiety)
        {
            TriggerDeath();
        }
    }

    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;

        StopAllCoroutines();
        StartCoroutine(FadeIn());
        playerStats.ResetAnxiety();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;

        SetUIInteractable(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        SetUIInteractable(true);
    }

    public void Retry()
    {
        if (playerStats != null)
            playerStats.ResetAnxiety();

        Time.timeScale = 1f;

        isDead = false;

        AudioReset();

        ResetUIState();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void AudioReset()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = 1f;
    }

    private void ResetUIState()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = 0f;
        SetUIInteractable(false);
    }

    private void SetUIInteractable(bool state)
    {
        if (canvasGroup == null) return;

        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }
}