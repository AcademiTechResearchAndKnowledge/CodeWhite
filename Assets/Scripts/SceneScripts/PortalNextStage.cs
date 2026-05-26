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

    public enum PortalOrientation
    {
        Vertical,
        Horizontal
    }

    private PortalOrientation orientation = PortalOrientation.Vertical;
    private Transform portalMesh;

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

    public void SetLevel(int value)
    {
        levelCounter = value;
    }

    public void SetExcludedScenes(string[] scenes)
    {
        excludedScenes = scenes;
    }

    public void SetForcedScene(string scene)
    {
        chosenScene = scene;
    }

    public void SetOrientation(PortalOrientation value)
    {
        orientation = value;
    }

    public void SetPortalMesh(Transform mesh)
    {
        portalMesh = mesh;
    }

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
            bool isLookUp = cam.name.ToLower().Contains("lookup");

            if (isLookUp)
            {
                lookUpCam = cam;
                cam.Priority = 0;
                cam.gameObject.SetActive(false);
                cam.LookAt = null;
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

        if (lookUpCam != null)
        {
            lookUpCam.Priority = 0;
            lookUpCam.gameObject.SetActive(false);
            lookUpCam.LookAt = null;
        }

        if (orientation == PortalOrientation.Vertical)
        {
            if (lookUpCam != null)
            {
                lookUpCam.gameObject.SetActive(true);
                lookUpCam.Priority = 100;
                lookUpCam.LookAt = lookTarget;
            }
        }
        else
        {
            if (mainCam != null)
            {
                mainCam.LookAt = portalMesh != null ? portalMesh : lookTarget;
                mainCam.Priority = 100;
            }
        }

        Vector3 startPos = player.position;
        Vector3 targetPos = transform.position;

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / duration);

            Vector3 pos;

            if (orientation == PortalOrientation.Horizontal)
            {
                pos = Vector3.Lerp(startPos, targetPos, t);
            }
            else
            {
                pos = Vector3.Lerp(startPos, targetPos, t);
                pos.y = startPos.y + Mathf.Lerp(0f, liftHeight, t);
            }

            player.position = pos;

            if (lookTarget != null)
            {
                if (orientation == PortalOrientation.Horizontal)
                    lookTarget.position = targetPos;
                else
                    lookTarget.position = player.position + Vector3.up * lookHeight;
            }

            UpdateFade(t);
            yield return null;
        }

        ForceFade();
        ResetCameras();

        yield return new WaitForSeconds(0.1f);

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
        CinemachineCamera[] cams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            bool isLookUp = cam.name.ToLower().Contains("lookup");

            if (isLookUp)
            {
                cam.Priority = 0;
                cam.LookAt = null;
                cam.gameObject.SetActive(false);
                continue;
            }

            cam.Priority = 100;
            cam.gameObject.SetActive(true);
        }

        if (mainCam != null)
        {
            mainCam.LookAt = null;
            mainCam.Priority = 100;
        }
    }

    private void LoadScene()
    {
        if (!string.IsNullOrEmpty(chosenScene))
        {
            SceneManager.LoadScene(chosenScene);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}