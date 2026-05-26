using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLoader : MonoBehaviour
{
    public void LoadIntroScene()
    {
        SceneManager.LoadScene("MainMenuDraft", LoadSceneMode.Single);
    }
}