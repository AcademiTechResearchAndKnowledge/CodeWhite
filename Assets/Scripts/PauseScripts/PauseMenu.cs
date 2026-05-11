using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        // REMOVED the continuous if(GameIsPaused) cursor spam from here
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        // Lock cursor back to the game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        // Unlock cursor ONCE for the UI menus
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;

        SceneManager.LoadScene("MainMenu");
    }
}