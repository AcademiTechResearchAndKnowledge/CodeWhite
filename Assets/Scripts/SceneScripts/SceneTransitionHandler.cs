using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance;

    public float fadeInSpeed = 2f;
    private Image fadeImage;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("SceneTransitionHandler initialized");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        GameObject fadeObj = GameObject.Find("FadeScreen");

        if (fadeObj != null)
            fadeImage = fadeObj.GetComponentInChildren<Image>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false;

            PlayerLook look = player.GetComponent<PlayerLook>();
            if (look != null)
                look.canLook = true;
        }

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        Debug.Log("Fade in started");

        while (fadeImage != null && fadeImage.color.a > 0f)
        {
            Color c = fadeImage.color;
            c.a = Mathf.MoveTowards(c.a, 0f, fadeInSpeed * Time.deltaTime);
            fadeImage.color = c;

            yield return null;
        }

        Debug.Log("Fade in complete");
    }
}