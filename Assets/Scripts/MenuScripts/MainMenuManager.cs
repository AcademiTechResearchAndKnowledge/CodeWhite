using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuUI;
    public GameObject settingsMenuUI;

    void Update()
    {
        // Listen for the Escape key being pressed
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // If the settings menu is currently turned on, close it!
            if (settingsMenuUI.activeSelf)
            {
                CloseSettings();
            }
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OpenSettings()
    {
        mainMenuUI.SetActive(false);
        settingsMenuUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsMenuUI.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}