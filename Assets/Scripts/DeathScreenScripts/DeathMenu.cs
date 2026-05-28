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
    }

    public void Retry()
    {
        StopDeathMusic();

        if (playerStats != null)
            playerStats.ResetAnxiety();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearInventory();

        if (ObjectiveInventoryManager.Instance != null)
            ObjectiveInventoryManager.Instance.ClearInventory();

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