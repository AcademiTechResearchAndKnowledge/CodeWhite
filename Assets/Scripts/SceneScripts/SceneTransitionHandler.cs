using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance;

    public float fadeInSpeed = 2f;
    private Image fadeImage;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this == null || gameObject == null) return;
        if (Instance != this) return;

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

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        if (fadeImage == null) yield break;

        while (fadeImage != null && fadeImage.color.a > 0f)
        {
            Color c = fadeImage.color;
            c.a = Mathf.MoveTowards(c.a, 0f, fadeInSpeed * Time.deltaTime);
            fadeImage.color = c;

            yield return null;
        }
    }
}