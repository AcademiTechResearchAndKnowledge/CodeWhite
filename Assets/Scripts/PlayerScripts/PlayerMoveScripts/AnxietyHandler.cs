using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class AnxietyHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Volume globalVolume;

    [Header("Audio")]
    [SerializeField] public AudioSource heartbeatAudio;
    [SerializeField] public AudioSource tinnitusAudio;
    [SerializeField] private float audioFadeOutDuration = 1.5f;

    [Header("Anxiety Object Settings")]
    [SerializeField] private LayerMask anxietyLayerMask;
    [SerializeField] private float gazeDetectionRange = 20f;
    [SerializeField] private float proximityRadius = 5f;
    [SerializeField] private float gazeAnxietyRate = 5f;
    [SerializeField] private float proximityAnxietyRate = 2f;

    [Header("Chase State")]
    [SerializeField] private bool isBeingChased = false;
    [SerializeField] private float chaseAnxietyRate = 4f;

    [Header("Anxiety Limits")]
    [Range(0f, 1f)]
    [SerializeField] private float safeAnxietyThreshold = 0.95f;

    [Header("Anxiety Cooldown Settings")]
    [SerializeField] private float decayDelay = 3f;
    [SerializeField] private float minDecayRate = 0.5f;
    [SerializeField] private float maxDecayRate = 5f;
    [SerializeField] private float decayAccelerationTime = 4f;

    [Header("Vignette Settings")]
    [SerializeField] private float vignetteThreshold = 0.7f;
    [SerializeField] private float vignetteMaxIntensity = 0.35f;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 6f;
    [SerializeField] private float pulseStrength = 0.05f;

    [Header("Blur Settings")]
    [SerializeField] private float blurMax = 1.5f;

    [Header("Color Anxiety (Red Pulse)")]
    [SerializeField] private float redPulseStrength = 0.4f;

    private Vignette _vignette;
    private DepthOfField _dof;
    private LiftGammaGain _color;

    private bool isLookingAtAnxietyObject = false;
    private bool isNearAnxietyObject = false;

    private float safeTimer = 0f;
    private bool isFadingOutAudio = false;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out _vignette);
            globalVolume.profile.TryGet(out _dof);
            globalVolume.profile.TryGet(out _color);
        }

        ResetAudioState();
    }

    private void OnEnable()
    {
        ResetAudioState();
    }

    private void ResetAudioState()
    {
        isFadingOutAudio = false;

        if (heartbeatAudio != null)
        {
            heartbeatAudio.Stop();
            heartbeatAudio.volume = 0f;
            heartbeatAudio.pitch = 1f;
        }

        if (tinnitusAudio != null)
        {
            tinnitusAudio.Stop();
            tinnitusAudio.volume = 0f;
        }
    }

    private void Update()
    {
        CheckGaze();
        CheckProximity();
        UpdateAnxiety();

        float anxietyPercent = (float)playerStats.Anxiety / (float)playerStats.MaxAnxiety;

        if (anxietyPercent >= 1f && !isFadingOutAudio)
        {
            StartCoroutine(FadeOutAllAudio());
        }

        UpdateHeartbeat(anxietyPercent);
        UpdateTinnitus(anxietyPercent);
        UpdateVisualEffects(anxietyPercent);
    }

    private IEnumerator FadeOutAllAudio()
    {
        isFadingOutAudio = true;

        float startHeartbeat = heartbeatAudio != null ? heartbeatAudio.volume : 0f;
        float startTinnitus = tinnitusAudio != null ? tinnitusAudio.volume : 0f;

        float time = 0f;

        while (time < audioFadeOutDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / audioFadeOutDuration;

            if (heartbeatAudio != null)
                heartbeatAudio.volume = Mathf.Lerp(startHeartbeat, 0f, t);

            if (tinnitusAudio != null)
                tinnitusAudio.volume = Mathf.Lerp(startTinnitus, 0f, t);

            yield return null;
        }

        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume = 0f;
            heartbeatAudio.Stop();
        }

        if (tinnitusAudio != null)
        {
            tinnitusAudio.volume = 0f;
            tinnitusAudio.Stop();
        }

        isFadingOutAudio = false;
    }

    private void UpdateHeartbeat(float anxietyPercent)
    {
        if (heartbeatAudio == null || isFadingOutAudio) return;

        float threshold = Mathf.Min(0.3f, safeAnxietyThreshold - 0.1f);

        if (anxietyPercent >= threshold)
        {
            if (!heartbeatAudio.isPlaying)
                heartbeatAudio.Play();

            heartbeatAudio.volume = Mathf.Lerp(0.2f, 1f, anxietyPercent);
            heartbeatAudio.pitch = Mathf.Lerp(1f, 2f, anxietyPercent);
        }
        else
        {
            heartbeatAudio.volume = Mathf.MoveTowards(heartbeatAudio.volume, 0f, Time.deltaTime);

            if (heartbeatAudio.volume <= 0f)
                heartbeatAudio.Stop();
        }
    }

    private void UpdateTinnitus(float anxietyPercent)
    {
        if (tinnitusAudio == null || isFadingOutAudio) return;

        float threshold = Mathf.Min(0.7f, safeAnxietyThreshold - 0.1f);

        if (anxietyPercent >= threshold)
        {
            if (!tinnitusAudio.isPlaying)
                tinnitusAudio.Play();

            tinnitusAudio.volume = Mathf.Lerp(0f, 0.8f, anxietyPercent);
        }
        else
        {
            tinnitusAudio.volume = Mathf.MoveTowards(tinnitusAudio.volume, 0f, Time.deltaTime * 2f);

            if (tinnitusAudio.volume <= 0f)
                tinnitusAudio.Stop();
        }
    }

    public void SetChaseState(bool state)
    {
        isBeingChased = state;
    }

    private void UpdateVisualEffects(float anxietyPercent)
    {
        float actualVignetteThreshold = Mathf.Min(vignetteThreshold, safeAnxietyThreshold - 0.05f);
        float t = Mathf.InverseLerp(actualVignetteThreshold, safeAnxietyThreshold, anxietyPercent);

        float rawPulse = Mathf.Sin(Time.time * (pulseSpeed + anxietyPercent * 4f));
        float smoothPulse = rawPulse * rawPulse;
        float pulse = smoothPulse * t;

        if (_vignette != null && _vignette.active)
        {
            float baseIntensity = Mathf.Lerp(0f, vignetteMaxIntensity, t);
            float intensity = baseIntensity + pulse * pulseStrength;

            _vignette.intensity.Override(Mathf.Lerp(_vignette.intensity.value, intensity, Time.deltaTime * 2f));
            _vignette.color.Override(Color.Lerp(Color.black, new Color(0.4f, 0f, 0f), t));
        }

        if (_dof != null && _dof.active)
        {
            if (isBeingChased)
            {
                float targetStart = Mathf.Lerp(0f, 2f, t);
                float targetEnd = Mathf.Lerp(3f, 0.5f, t);
                float targetRadius = Mathf.Lerp(0f, blurMax, t) + pulse * 0.5f;

                _dof.gaussianStart.Override(Mathf.Lerp(_dof.gaussianStart.value, targetStart, Time.deltaTime * 2f));
                _dof.gaussianEnd.Override(Mathf.Lerp(_dof.gaussianEnd.value, targetEnd, Time.deltaTime * 2f));
                _dof.gaussianMaxRadius.Override(Mathf.Lerp(_dof.gaussianMaxRadius.value, targetRadius, Time.deltaTime * 2f));
            }
            else
            {
                _dof.gaussianStart.Override(Mathf.Lerp(_dof.gaussianStart.value, 10f, Time.deltaTime * 2f));
                _dof.gaussianEnd.Override(Mathf.Lerp(_dof.gaussianEnd.value, 20f, Time.deltaTime * 2f));
                _dof.gaussianMaxRadius.Override(Mathf.Lerp(_dof.gaussianMaxRadius.value, 0f, Time.deltaTime * 2f));
            }
        }

        if (_color != null && _color.active)
        {
            float colorTriggerThreshold = Mathf.Min(0.9f, safeAnxietyThreshold - 0.05f);

            if (anxietyPercent >= colorTriggerThreshold)
            {
                float t90 = Mathf.InverseLerp(colorTriggerThreshold, safeAnxietyThreshold, anxietyPercent);

                float rawPulse90 = Mathf.Sin(Time.time * (pulseSpeed + anxietyPercent * 4f));
                float smoothPulse90 = rawPulse90 * rawPulse90;
                float pulse90 = smoothPulse90 * t90;

                Vector3 pulseRed = new Vector3(1f, 0.15f, 0.15f) * (pulse90 * redPulseStrength);

                Vector4 lift = new Vector4(-0.02f, -0.02f, -0.02f, 0f);
                Vector4 gamma = new Vector4(1f + pulseRed.x, 1f + pulseRed.y, 1f + pulseRed.z, 0f);
                Vector4 gain = new Vector4(1f + pulseRed.x, 1f + pulseRed.y, 1f + pulseRed.z, 0f);

                _color.lift.Override(Vector4.Lerp(_color.lift.value, lift, Time.deltaTime * 2f));
                _color.gamma.Override(Vector4.Lerp(_color.gamma.value, gamma, Time.deltaTime * 2f));
                _color.gain.Override(Vector4.Lerp(_color.gain.value, gain, Time.deltaTime * 2f));
            }
            else
            {
                Vector4 neutralLift = new Vector4(0f, 0f, 0f, 0f);
                Vector4 neutralGamma = new Vector4(1f, 1f, 1f, 0f);
                Vector4 neutralGain = new Vector4(1f, 1f, 1f, 0f);

                _color.lift.Override(Vector4.Lerp(_color.lift.value, neutralLift, Time.deltaTime * 2f));
                _color.gamma.Override(Vector4.Lerp(_color.gamma.value, neutralGamma, Time.deltaTime * 2f));
                _color.gain.Override(Vector4.Lerp(_color.gain.value, neutralGain, Time.deltaTime * 2f));
            }
        }
    }

    private void CheckGaze()
    {
        isLookingAtAnxietyObject = false;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, gazeDetectionRange))
        {
            if (((1 << hit.collider.gameObject.layer) & anxietyLayerMask) != 0)
                isLookingAtAnxietyObject = true;
        }
    }

    private void CheckProximity()
    {
        isNearAnxietyObject =
            Physics.OverlapSphere(transform.position, proximityRadius, anxietyLayerMask).Length > 0;
    }

    private void UpdateAnxiety()
    {
        float anxietyPercent = (float)playerStats.Anxiety / (float)playerStats.MaxAnxiety;
        bool isAnxietyTriggered = isLookingAtAnxietyObject || isBeingChased || isNearAnxietyObject;

        if (isAnxietyTriggered)
        {
            safeTimer = 0f;

            if (anxietyPercent < safeAnxietyThreshold)
            {
                float rateToApply = 0f;

                if (isLookingAtAnxietyObject) rateToApply = gazeAnxietyRate;
                else if (isBeingChased) rateToApply = chaseAnxietyRate;
                else if (isNearAnxietyObject) rateToApply = proximityAnxietyRate;

                playerStats.AddStat(StatType.ANX, rateToApply * Time.deltaTime);
            }
        }
        else
        {
            safeTimer += Time.deltaTime;

            if (safeTimer >= decayDelay)
            {
                float timeDecaying = safeTimer - decayDelay;
                float accelerationProgress = Mathf.Clamp01(timeDecaying / decayAccelerationTime);
                float currentDecayRate = Mathf.Lerp(minDecayRate, maxDecayRate, accelerationProgress);

                playerStats.SubtractStat(StatType.ANX, currentDecayRate * Time.deltaTime);
            }
        }
    }
}