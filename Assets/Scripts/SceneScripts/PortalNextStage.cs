using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PortalNextStage : MonoBehaviour
{
    public string playerTag = "Player";
    public float duration = 6f;
    public float liftHeight = 2f;
    public float lookHeight = 10f;

    [Header("Boss Rule")]
    [SerializeField] private int bossInterval = 10;

    [Header("Fade")]
    [SerializeField] private float fadeStart = 0.6f;
    [SerializeField] private float fadeSpeed = 1f;

    private int levelCounter;

    private string chosenScene;
    private string[] allScenes;

    private Image fadeImage;
    private GameObject fadeObject;

    private bool used;

    private PlayerLook playerLook;
    private CinemachineCamera mainCam;
    private CinemachineCamera lookUpCam;
    private Transform lookTarget;

    public void SetLevel(int value)
    {
        levelCounter = value;
        Debug.Log("PORTAL LEVEL SET = " + levelCounter);
    }

    private void Awake()
    {
        playerLook = Object.FindFirstObjectByType<PlayerLook>();

        CinemachineCamera[] cams =
            Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            if (mainCam == null || cam.Priority > mainCam.Priority)
                mainCam = cam;

            if (cam.name.ToLower().Contains("lookup"))
                lookUpCam = cam;
        }

        GameObject target = new GameObject("AutoLookTarget");
        lookTarget = target.transform;

        if (lookUpCam != null)
            lookUpCam.gameObject.SetActive(false);

        CacheScenes();
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

        Debug.Log("Portal initialized");
    }

    private void CacheScenes()
    {
        int count = SceneManager.sceneCountInBuildSettings;
        allScenes = new string[count];

        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            allScenes[i] = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        Debug.Log("Scenes cached: " + count);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;

        ChooseScene();

        Debug.Log("Portal triggered → " + chosenScene);

        StartCoroutine(Sequence(other.transform));
    }

    private void ChooseScene()
    {
        if (allScenes == null || allScenes.Length == 0)
        {
            chosenScene = "";
            return;
        }

        bool isBoss = (levelCounter % bossInterval == 0);

        if (isBoss)
        {
            int index = (levelCounter / bossInterval - 1) % allScenes.Length;
            chosenScene = allScenes[index];
            Debug.Log("BOSS SCENE → " + chosenScene);
        }
        else
        {
            chosenScene = allScenes[Random.Range(0, allScenes.Length)];
            Debug.Log("NORMAL SCENE → " + chosenScene);
        }
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

        if (lookUpCam != null)
        {
            lookUpCam.gameObject.SetActive(true);
            lookUpCam.Priority = 100;
            lookUpCam.LookAt = lookTarget;
        }

        if (mainCam != null)
            mainCam.Priority = 0;

        Vector3 startPos = player.position;
        Vector3 targetPos = transform.position;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            pos.y = startPos.y + Mathf.Lerp(0f, liftHeight, t);
            player.position = pos;

            if (lookTarget != null)
                lookTarget.position = player.position + Vector3.up * lookHeight;

            UpdateFade(t);

            yield return null;
        }

        ForceFade();

        ResetCameras();

        LoadScene();
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
        CinemachineCamera[] cams =
            Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            if (cam.name.ToLower().Contains("lookup"))
            {
                cam.Priority = 0;
                cam.LookAt = null;
                cam.gameObject.SetActive(false);
            }
            else
            {
                cam.Priority = 100;
                cam.gameObject.SetActive(true);
            }
        }

        if (lookUpCam != null)
        {
            lookUpCam.Priority = 0;
            lookUpCam.LookAt = null;
            lookUpCam.gameObject.SetActive(false);
        }

        if (mainCam != null)
            mainCam.Priority = 100;
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(chosenScene))
        {
            SceneManager.LoadScene(chosenScene);
        }
        else
        {
            Debug.LogWarning("Fallback scene load");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}