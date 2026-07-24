using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject settingsMenuUI;

    private bool hintBookWasOpenOnPause = false;

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
        EventSystem.current.SetSelectedGameObject(null);

        if (hintBookWasOpenOnPause)
        {
            hintBookWasOpenOnPause = false;
            var hintBook = FindFirstObjectByType<HintBookController>();
            if (hintBook != null)
            {
                hintBook.CloseHintBook();
                return;
            }
        }

        var zoom = FindFirstObjectByType<objectZoom>();
        var bookUI = FindFirstObjectByType<BookInspectionUI>();

        // Check if either a puzzle is active OR the book is open
        if ((zoom != null && zoom.isInPuzzle) || (bookUI != null && bookUI.IsOpen()))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Pause()
    {
        // Removed the BookInspectionUI forcing logic here so it stays open in the background

        var hintBook = FindFirstObjectByType<HintBookController>();
        if (hintBook != null && hintBook.IsOpen())
        {
            hintBookWasOpenOnPause = true;
            hintBook.CloseHintBookFromPause();
        }

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenSettings()
    {
        pauseMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        AudioListener.pause = false;

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

        SceneManager.LoadScene("MainMenuDraft");
    }
}