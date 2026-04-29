using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour
{
    public void LoadIntroScene()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}