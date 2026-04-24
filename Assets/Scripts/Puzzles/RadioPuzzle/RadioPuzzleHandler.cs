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

    public float minTargetFrequency = 88.0f;
    public float maxTargetFrequency = 108.0f;
    public float minOminousFrequency = 88.0f;
    public float maxOminousFrequency = 108.0f;

    public float submitDelay = 2f;
    public float ominousPushForce = 5f;
    public float verticalPushForce = 2f;

    [SerializeField] private float maxFrequencyGain = 3f;
    [SerializeField] private float damping = 2f;

    [SerializeField] private float maxFilmGrain = 1f;
    [SerializeField] private float grainDamping = 2f;

    [SerializeField] private float exitFadeSpeed = 2.5f;

    private CinemachineCamera cineCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private Volume postProcessVolume;
    private FilmGrain filmGrain;

    private float targetFrequency;
    private float ominousFrequency;
    private float submitTimer;
    private float lastFrequency;

    private bool puzzleActive = false;
    private bool fadingOut = false;

    void Start()
    {
        GetActiveCinemachineCamera();
        GetMainCameraVolume();
        ResetPuzzleImmediate();
    }

    void Update()
    {
        if (puzzleActive && objZoom.isInPuzzle)
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
            submitTimer = 0f;
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
    }


    void HandleFadeOut()
    {
        if (!fadingOut) return;

        float t = Time.deltaTime * exitFadeSpeed;

        if (noise != null)
            noise.FrequencyGain = Mathf.Lerp(noise.FrequencyGain, 0f, t);

        if (filmGrain != null)
            filmGrain.intensity.value = Mathf.Lerp(filmGrain.intensity.value, 0f, t);


    }


    public void SubmitFrequency()
    {
        if (Mathf.Abs(knobValue.frequency - ominousFrequency) < 0.1f)
        {
            StartCoroutine(ExitAndPushPlayer());
            return;
        }

        if (Mathf.Abs(knobValue.frequency - targetFrequency) < 0.1f)
        {
            knobValue.submitted = true;
        }
    }

    IEnumerator ExitAndPushPlayer()
    {
        puzzleActive = false;
        fadingOut = true;

        knobValue.dragging = false;
        objZoom.isInPuzzle = false;

        objZoom.ExitPuzzle();

        yield return new WaitForFixedUpdate();

        Rigidbody rb = objZoom.playerController?.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;

            Vector3 dir = objZoom.transform.forward.normalized;
            dir.y = verticalPushForce / ominousPushForce;

            rb.AddForce(dir * ominousPushForce, ForceMode.Impulse);
        }


        ResetPuzzleImmediate();
    }


    void ResetPuzzleImmediate()
    {
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

        Debug.Log($"[RESET INSTANT] T:{targetFrequency:F1} | O:{ominousFrequency:F1}");
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
            postProcessVolume.profile.TryGet(out filmGrain);
    }
}