using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalNextStage : MonoBehaviour
{
    public string playerTag = "Player";

    private float duration = 6f;
    private float liftHeight = 2f;
    private float lookHeight = 10f;
    private float fadeStart = 0.6f;
    private float fadeSpeed = 1f;

    public float fadeOutDuration = 1f;

    public enum PortalOrientation { Vertical, Horizontal }
    private PortalOrientation orientation = PortalOrientation.Vertical;

    [SerializeField] private string[] excludedScenes;

    private int levelCounter;
    private string chosenScene;

    private Image fadeImage;
    private GameObject fadeObject;

    private bool used;

    private PlayerLook playerLook;
    private CinemachineCamera mainCam;
    private CinemachineCamera lookUpCam;
    private Transform lookTarget;

    public void SetLevel(int value) => levelCounter = value;
    public void SetExcludedScenes(string[] scenes) => excludedScenes = scenes;
    public void SetForcedScene(string scene) => chosenScene = scene;
    public void SetOrientation(PortalOrientation value) => orientation = value;

    public void SetSequenceSettings(float dur, float lift, float look, float fadeSt, float fadeSp)
    {
        duration = dur;
        liftHeight = lift;
        lookHeight = look;
        fadeStart = fadeSt;
        fadeSpeed = fadeSp;
    }

    private void Awake()
    {
        playerLook = Object.FindFirstObjectByType<PlayerLook>();

        CinemachineCamera[] cams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            bool isLookUp = cam.name.IndexOf("lookup", System.StringComparison.OrdinalIgnoreCase) >= 0;

            if (isLookUp)
            {
                lookUpCam = cam;
                lookUpCam.Priority = -10;
                lookUpCam.LookAt = null;
                continue;
            }

            if (mainCam == null || cam.Priority > mainCam.Priority)
                mainCam = cam;
        }

        GameObject target = new GameObject("AutoLookTarget");
        lookTarget = target.transform;
    }

    private void Start()
    {
        StartCoroutine(InitFade());
    }

    private IEnumerator InitFade()
    {
        yield return null;

        fadeObject = GameObject.Find("FadeScreen");

        if (fadeObject != null)
        {
            fadeImage = fadeObject.GetComponentInChildren<Image>();

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 0f;
                fadeImage.color = c;
            }

            fadeObject.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;
        StartCoroutine(Sequence(other.transform));
    }

    private IEnumerator Sequence(Transform player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (playerLook != null)
            playerLook.canLook = false;

        if (orientation == PortalOrientation.Vertical)
        {
            if (mainCam != null)
                mainCam.Priority = 0;

            if (lookUpCam != null)
            {
                lookUpCam.gameObject.SetActive(true);
                lookUpCam.LookAt = lookTarget;
                lookUpCam.Priority = 10000;
            }
        }
        else
        {
            if (mainCam != null)
                mainCam.LookAt = null;
        }

        yield return null;
        yield return null;

        Vector3 startPos = player.position;
        Vector3 targetPos = transform.position;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

            if (orientation == PortalOrientation.Vertical)
                pos.y = startPos.y + Mathf.Lerp(0f, liftHeight, t);

            player.position = pos;

            if (lookTarget != null)
            {
                lookTarget.position = orientation == PortalOrientation.Horizontal
                    ? targetPos
                    : player.position + Vector3.up * lookHeight;
            }

            UpdateFade(t);
            yield return null;
        }

        ForceFade();

        yield return new WaitForSeconds(0.2f);

        ResetCameras();

        SceneManager.sceneLoaded += OnSceneLoaded;
        LoadScene();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj == null) return;

        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        PlayerLook look = playerObj.GetComponent<PlayerLook>();
        if (look != null)
            look.canLook = true;

        fadeObject = GameObject.Find("FadeScreen");

        if (fadeObject != null)
        {
            fadeImage = fadeObject.GetComponentInChildren<Image>();

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = 1f;
                fadeImage.color = c;

                fadeObject.SetActive(true);
                FadeOutHelper.Run(fadeImage, fadeOutDuration);
            }
        }
    }

    private void UpdateFade(float t)
    {
        if (fadeImage == null) return;

        float fadeT = Mathf.InverseLerp(fadeStart, 1f, t);
        float alpha = Mathf.Clamp01(fadeT * fadeSpeed);

        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;
    }

    private void ForceFade()
    {
        if (fadeImage == null) return;

        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;
    }

    private void ResetCameras()
    {
        CinemachineCamera[] cams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            cam.LookAt = null;
            cam.Priority = -100;
        }

        if (mainCam != null)
            mainCam.Priority = 20;

        if (lookUpCam != null)
        {
            lookUpCam.Priority = -10;
            lookUpCam.LookAt = null;
            lookUpCam.gameObject.SetActive(false);
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(chosenScene))
            SceneManager.LoadScene(chosenScene);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

public static class FadeOutHelper
{
    public static void Run(Image image, float fadeDuration)
    {
        GameObject host = new GameObject("_FadeOutRunner");
        FadeOutRunner runner = host.AddComponent<FadeOutRunner>();
        runner.Begin(image, fadeDuration);
    }
}

public class FadeOutRunner : MonoBehaviour
{
    public void Begin(Image image, float duration)
    {
        StartCoroutine(FadeOut(image, duration));
    }

    private IEnumerator FadeOut(Image image, float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);

            Color c = image.color;
            c.a = alpha;
            image.color = c;

            yield return null;
        }

        Color final = image.color;
        final.a = 0f;
        image.color = final;

        Destroy(gameObject);
    }
}