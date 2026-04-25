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

    [Header("Scene")]
    public string nextSceneName;

    [Header("Fade")]
    public float fadeStartPoint = 0.3f;

    private Image fadeImage;
    private GameObject fadeObject;

    private bool used = false;

    private PlayerLook playerLook;
    private CinemachineCamera mainCam;
    private CinemachineCamera lookUpCam;
    private Transform lookTarget;

    private void Awake()
    {
        playerLook = Object.FindFirstObjectByType<PlayerLook>();

        CinemachineCamera[] cams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            if (mainCam == null || cam.Priority > mainCam.Priority)
                mainCam = cam;

            if (cam.name.ToLower().Contains("lookup"))
                lookUpCam = cam;
        }

        if (lookUpCam == null && cams.Length > 1)
            lookUpCam = cams[1];

        GameObject target = new GameObject("AutoLookTarget");
        lookTarget = target.transform;
    }

    private void Start()
    {
        fadeObject = GameObject.Find("FadeScreen");

        if (fadeObject == null)
        {
            Debug.LogWarning("[PortalNextStage] FadeScreen not found.");
            return;
        }

        fadeImage = fadeObject.GetComponentInChildren<Image>();

        if (fadeImage == null)
        {
            Debug.LogWarning("[PortalNextStage] Image not found under FadeScreen.");
            return;
        }

        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        fadeObject.SetActive(true);

        Debug.Log("[PortalNextStage] Fade system initialized.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;

        Debug.Log("[PortalNextStage] Trigger entered by player.");

        StartCoroutine(Sequence(other.transform));
    }

    private IEnumerator Sequence(Transform player)
    {
        Debug.Log("[PortalNextStage] Sequence started.");

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
            lookUpCam.Priority = 100;

        if (mainCam != null)
            mainCam.Priority = 0;

        if (lookUpCam != null)
            lookUpCam.LookAt = lookTarget;

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

            float fadeT = Mathf.InverseLerp(fadeStartPoint, 1f, t);
            fadeT = Mathf.Clamp01(fadeT);

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = fadeT;
                fadeImage.color = c;
            }

            yield return null;
        }

        player.position = transform.position + Vector3.up * liftHeight;

        Debug.Log("[PortalNextStage] Movement finished, loading scene.");

        yield return new WaitForSeconds(0.3f);

        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("[PortalNextStage] Loading scene: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("[PortalNextStage] Loading next scene index.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}