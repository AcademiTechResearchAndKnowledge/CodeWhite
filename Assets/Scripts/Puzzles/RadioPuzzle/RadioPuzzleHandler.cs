using TMPro;
using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RadioPuzzleHandler : MonoBehaviour
{
    [SerializeField] public TextMeshPro frequencyText;
    [SerializeField] public KnobRotate knobValue;
    [SerializeField] public objectZoom objZoom;
    public RandomPortalSpawner RPS;

    public float minTargetFrequency = 88.0f;
    public float maxTargetFrequency = 108.0f;
    public float minOminousFrequency = 88.0f;
    public float maxOminousFrequency = 108.0f;

    public float submitDelay = 2f;
    public float ominousPushForce = 5f;
    public float verticalPushForce = 2f;

    [SerializeField] private int requiredSuccesses = 3;
    private int currentSuccesses = 0;

    [SerializeField] private float maxFrequencyGain = 3f;
    [SerializeField] private float damping = 2f;

    [SerializeField] private float maxFilmGrain = 1f;
    [SerializeField] private float grainDamping = 2f;

    [SerializeField] private float maxVignette = 0.5f;
    [SerializeField] private float vignetteDamping = 2f;

    [SerializeField] private float exitFadeSpeed = 2.5f;

    private CinemachineCamera cineCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private Volume postProcessVolume;
    private FilmGrain filmGrain;
    private Vignette vignette;

    private float targetFrequency;
    private float ominousFrequency;
    private float submitTimer;
    private float lastFrequency;

    private bool puzzleActive = false;
    private bool fadingOut = false;
    private bool puzzleCompleted = false;

    void Start()
    {
        GetActiveCinemachineCamera();
        GetMainCameraVolume();
        ResetPuzzleImmediate();
    }

    void Update()
    {
        if (puzzleActive && objZoom.isInPuzzle && !puzzleCompleted)
        {
            HandlePuzzle();
        }

        HandleFadeOut();
    }

    void HandlePuzzle()
    {
        if (!knobValue.dragging || knobValue.submitted)
            return;

        UpdateFrequency();

        float dist = Mathf.Abs(knobValue.frequency - ominousFrequency);

        if (Mathf.Abs(knobValue.frequency - lastFrequency) > 0.001f)
        {
            submitTimer = submitDelay;
            lastFrequency = knobValue.frequency;
        }

        submitTimer -= Time.deltaTime;

        if (submitTimer <= 0f)
        {
            SubmitFrequency();
            submitTimer = submitDelay;
        }

        ApplyEffects(dist);
    }

    void ApplyEffects(float dist)
    {
        if (noise != null)
        {
            noise.FrequencyGain = dist < 0.5f
                ? Mathf.Lerp(0f, maxFrequencyGain, 1f - dist / 0.5f)
                : Mathf.Lerp(noise.FrequencyGain, 0f, Time.deltaTime * damping);
        }

        if (filmGrain != null)
        {
            filmGrain.intensity.value = dist < 0.5f
                ? Mathf.Lerp(0f, maxFilmGrain, 1f - dist / 0.5f)
                : Mathf.Lerp(filmGrain.intensity.value, 0f, Time.deltaTime * grainDamping);
        }

        if (vignette != null)
        {
            vignette.intensity.value = dist < 0.5f
                ? Mathf.Lerp(0f, maxVignette, 1f - dist / 0.5f)
                : Mathf.Lerp(vignette.intensity.value, 0f, Time.deltaTime * vignetteDamping);
        }
    }

    void HandleFadeOut()
    {
        if (!fadingOut) return;

        float t = Time.deltaTime * exitFadeSpeed;

        if (noise != null)
            noise.FrequencyGain = Mathf.Lerp(noise.FrequencyGain, 0f, t);

        if (filmGrain != null)
            filmGrain.intensity.value = Mathf.Lerp(filmGrain.intensity.value, 0f, t);

        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0f, t);
    }

    public void SubmitFrequency()
    {
        float current = knobValue.frequency;

        Debug.Log($"[SUBMIT] Player: {current:F1} | Target: {targetFrequency:F1} | Ominous: {ominousFrequency:F1}");

        if (Mathf.Abs(current - ominousFrequency) < 0.1f)
        {
            Debug.Log($"[OMINOUS HIT] {current:F1}");
            StartCoroutine(ExitAndPushPlayer());
            return;
        }

        if (Mathf.Abs(current - targetFrequency) < 0.1f)
        {
            currentSuccesses++;
            Debug.Log($"[CORRECT] {currentSuccesses}/{requiredSuccesses}");

            if (currentSuccesses >= requiredSuccesses)
            {
                CompletePuzzle();
            }
            else
            {
                StartCoroutine(NextRound());
            }

            knobValue.submitted = true;
        }
    }

    IEnumerator ExitAndPushPlayer()
    {
        puzzleActive = false;
        fadingOut = true;

        knobValue.dragging = false;

        objZoom.ExitPuzzle();
        objZoom.isInPuzzle = false;

        yield return new WaitForFixedUpdate();

        Rigidbody rb = objZoom.playerController?.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;

            Vector3 dir = objZoom.transform.forward.normalized;
            dir.y = verticalPushForce / ominousPushForce;

            rb.AddForce(dir * ominousPushForce, ForceMode.Impulse);
        }

        StartCoroutine(NextRound());
    }

    IEnumerator NextRound()
    {
        yield return new WaitForSeconds(1f);
        ResetPuzzleImmediate();
    }

    void CompletePuzzle()
    {
        puzzleCompleted = true;
        puzzleActive = false;

        RPS.SpawnPortalRandom();

        Debug.Log("[PUZZLE COMPLETED]");

        objZoom.InteractZoomObj();
    }

    void ResetPuzzleImmediate()
    {
        if (puzzleCompleted) return;

        puzzleActive = true;

        knobValue.ResetKnob();
        knobValue.dragging = false;
        knobValue.submitted = false;

        targetFrequency = Random.Range(minTargetFrequency, maxTargetFrequency);

        do
        {
            ominousFrequency = Random.Range(minOminousFrequency, maxOminousFrequency);
        }
        while (Mathf.Abs(ominousFrequency - targetFrequency) < 0.1f);

        submitTimer = submitDelay;
        lastFrequency = knobValue.frequency;

        knobValue.frequency = knobValue.minFrequency;
        UpdateFrequency();

        Debug.Log($"[ROUND START] {currentSuccesses}/{requiredSuccesses}");
    }

    void UpdateFrequency()
    {
        frequencyText.text = knobValue.frequency.ToString("F1") + " MHz";
    }

    void GetActiveCinemachineCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        CinemachineBrain brain = cam.GetComponent<CinemachineBrain>();
        if (brain == null || brain.ActiveVirtualCamera == null) return;

        cineCamera = brain.ActiveVirtualCamera as CinemachineCamera;

        if (cineCamera != null)
            noise = cineCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
    }

    void GetMainCameraVolume()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        postProcessVolume = cam.GetComponent<Volume>();

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out filmGrain);
            postProcessVolume.profile.TryGet(out vignette);
        }
    }
}