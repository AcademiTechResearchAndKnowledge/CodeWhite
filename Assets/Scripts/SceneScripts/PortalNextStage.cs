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
    }

    private void Start()
    {
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

            float fadeT = Mathf.InverseLerp(fadeStartPoint, 1f, t);

            if (fadeImage != null)
            {
                Color c = fadeImage.color;
                c.a = Mathf.Clamp01(fadeT);
                fadeImage.color = c;
            }

            yield return null;
        }

        player.position = transform.position + Vector3.up * liftHeight;

        yield return new WaitForSeconds(0.3f);

        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}