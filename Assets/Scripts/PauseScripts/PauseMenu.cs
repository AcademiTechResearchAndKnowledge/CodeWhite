using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // CRITICAL: Added this to talk to the UI Event System

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (settingsMenuUI.activeSelf)
            {
                CloseSettings();
            }
            else if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Clear selection just to be safe when unpausing
        EventSystem.current.SetSelectedGameObject(null);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear any leftover selections from the last time we paused
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);

        // Clear selection so the Settings menu buttons start fresh
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);

        // THIS FIXES THE BUG: Clear the stuck "Settings" button selection
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        AudioListener.pause = false;

        // --- DESTROY ALL PERSISTENT OBJECTS BEFORE LOADING MENU ---
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

        SceneManager.LoadScene("MainMenu");
    }
}