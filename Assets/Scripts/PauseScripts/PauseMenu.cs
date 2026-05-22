using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // If settings are open, back out to the pause menu first
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

        // Resume all audio in the scene
        AudioListener.pause = false;

        // Lock cursor back to the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        // Freeze all audio in the scene (including the anxiety SFX)
        AudioListener.pause = true;

        // Unlock cursor ONCE for the UI menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;

        // CRITICAL: Unpause the audio before loading the new scene!
        // Otherwise, the main menu will be completely silent.
        AudioListener.pause = false;

        // --- DESTROY ALL PERSISTENT OBJECTS BEFORE LOADING MENU ---

        // 1. Destroy the Player
        PersistAcrossScenes player = FindFirstObjectByType<PersistAcrossScenes>();
        if (player != null) Destroy(player.gameObject);

        // 2. Destroy the UI
        PersistentUI ui = FindFirstObjectByType<PersistentUI>();
        if (ui != null) Destroy(ui.gameObject);

        // 3. Destroy the Run Manager
        RunManager runManager = FindFirstObjectByType<RunManager>();
        if (runManager != null) Destroy(runManager.gameObject);

        // 4. Destroy the Inventories
        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null) Destroy(inv.gameObject);

        ObjectiveInventoryManager objInv = FindFirstObjectByType<ObjectiveInventoryManager>();
        if (objInv != null) Destroy(objInv.gameObject);

        // 5. Destroy Portal Spawner
        RandomPortalSpawner spawner = FindFirstObjectByType<RandomPortalSpawner>();
        if (spawner != null) Destroy(spawner.gameObject);

        // Now it is completely safe to load the Main Menu!
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }
}