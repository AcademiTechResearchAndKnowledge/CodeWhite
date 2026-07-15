using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class DeathMenu : MonoBehaviour
{
    public static Action OnPlayerRestart;

    [Header("UI")]
    [SerializeField] private GameObject deathUI;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Red BG")]
    [SerializeField] private CanvasGroup redBg;
    [SerializeField] private float redFlashDuration = 0.2f;

    [Header("Tip Text")]
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private float tipFadeInDelay = 0.3f;
    [SerializeField] private float tipFadeInDuration = 1f;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource deathMusicSource;

    [Header("Refs")]
    [SerializeField] private PlayerStats playerStats;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isDead;
    private bool canCheckDeath;
    private float maxMusicVolume = 1f;

    private static DeathMenu instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Time.timeScale = 1f;

        isDead = false;
        canCheckDeath = false;

        if (playerStats == null)
            playerStats = FindFirstObjectByType<PlayerStats>();

        if (deathMusicSource != null)
            maxMusicVolume = deathMusicSource.volume;

        ResetUIState();
        AudioReset();

        StartCoroutine(EnableDeathCheckNextFrame());
    }

    private void OnEnable()
    {
        AudioReset();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

        AudioListener.pause = true;

        if (deathMusicSource != null)
        {
            deathMusicSource.ignoreListenerPause = true;
        }

        StopAllCoroutines();
        StartCoroutine(DeathSequence());

        playerStats.ResetAnxiety();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    private IEnumerator DeathSequence()
    {
        if (redBg != null)
        {
            redBg.gameObject.SetActive(true);
            redBg.alpha = 1f;
            yield return new WaitForSecondsRealtime(redFlashDuration);
        }

        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;

        SetUIInteractable(false);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (tipText != null)
        {
            Color c = tipText.color;
            c.a = 0f;
            tipText.color = c;
        }

        if (deathMusicSource != null)
        {
            deathMusicSource.volume = 0f;
            deathMusicSource.Play();
        }

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(t / fadeDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = progress;

            if (deathMusicSource != null)
                deathMusicSource.volume = progress * maxMusicVolume;

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (deathMusicSource != null)
            deathMusicSource.volume = maxMusicVolume;

        SetUIInteractable(true);

        if (tipText != null)
            StartCoroutine(FadeInTipText());
    }

    private IEnumerator FadeInTipText()
    {
        yield return new WaitForSecondsRealtime(tipFadeInDelay);

        float t = 0f;

        Color c = tipText.color;

        while (t < tipFadeInDuration)
        {
            t += Time.unscaledDeltaTime;

            float alpha = Mathf.Clamp01(t / tipFadeInDuration);

            c.a = alpha;
            tipText.color = c;

            yield return null;
        }

        c.a = 1f;
        tipText.color = c;
    }

    public void Retry()
    {
        OnPlayerRestart?.Invoke();

        StopDeathMusic();

        if (playerStats != null)
            playerStats.ResetAnxiety();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearInventory();

        if (ObjectiveInventoryManager.Instance != null)
            ObjectiveInventoryManager.Instance.ClearInventory();

        PlayerReferences playerRefs = FindFirstObjectByType<PlayerReferences>();

        if (playerRefs != null)
        {
            if (playerRefs.movementScript != null) playerRefs.movementScript.enabled = true;
            if (playerRefs.playerLook != null) playerRefs.playerLook.enabled = true;

            if (playerRefs.flashlightScript != null)
            {
                playerRefs.flashlightScript.enabled = true;
                playerRefs.flashlightScript.ForceTurnOff();
            }

            if (playerRefs.bodyMeshRenderer != null) playerRefs.bodyMeshRenderer.enabled = true;
            if (playerRefs.playerCollider != null) playerRefs.playerCollider.enabled = true;

            if (playerRefs.rb != null)
            {
                playerRefs.rb.isKinematic = false;
                playerRefs.rb.linearVelocity = Vector3.zero;
                playerRefs.rb.angularVelocity = Vector3.zero;
            }

            if (playerRefs.playerCam != null)
                playerRefs.playerCam.Priority = 100;
        }
        else
        {
            PlayerMovement playerController = FindFirstObjectByType<PlayerMovement>();
            PlayerLook playerLook = FindFirstObjectByType<PlayerLook>();
            Flashlight fl = FindFirstObjectByType<Flashlight>();

            if (playerController != null) playerController.enabled = true;
            if (playerLook != null) playerLook.enabled = true;
            if (fl != null)
            {
                fl.enabled = true;
                fl.ForceTurnOff();
            }
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        isDead = false;

        AudioReset();
        ResetUIState();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        StopDeathMusic();

        if (playerStats != null)
            playerStats.ResetAnxiety();

        Time.timeScale = 1f;

        PersistAcrossScenes player = FindFirstObjectByType<PersistAcrossScenes>();
        if (player != null) Destroy(player.gameObject);

        PersistentUI ui = FindFirstObjectByType<PersistentUI>();
        if (ui != null) Destroy(ui.gameObject);

        RunManager runManager = FindFirstObjectByType<RunManager>();
        if (runManager != null) Destroy(runManager.gameObject);

        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null) Destroy(inv.gameObject);

        ObjectiveInventoryManager objInv = FindFirstObjectByType<ObjectiveInventoryManager>();
        if (objInv != null) Destroy(objInv.gameObject);

        RandomPortalSpawner spawner = FindFirstObjectByType<RandomPortalSpawner>();
        if (spawner != null) Destroy(spawner.gameObject);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioReset();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetUIState();

        isDead = false;
        canCheckDeath = false;

        StartCoroutine(EnableDeathCheckNextFrame());
    }

    private void AudioReset()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = 1f;
    }

    private void StopDeathMusic()
    {
        if (deathMusicSource != null && deathMusicSource.isPlaying)
        {
            deathMusicSource.Stop();
        }
    }

    private void ResetUIState()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (tipText != null)
        {
            Color c = tipText.color;
            c.a = 0f;
            tipText.color = c;
        }

        if (redBg != null)
        {
            redBg.alpha = 0f;
            redBg.gameObject.SetActive(false);
        }
    }

    private void SetUIInteractable(bool state)
    {
        if (canvasGroup == null) return;

        canvasGroup.interactable = state;
        canvasGroup.blocksRaycasts = state;
    }
}