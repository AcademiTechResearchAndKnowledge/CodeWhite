using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnxietyHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Volume globalVolume;

    [Header("Audio")]
    [SerializeField] private AudioSource heartbeatAudio;
    [SerializeField] private AudioSource tinnitusAudio;

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
    [Tooltip("The max percentage anxiety can reach passively (e.g., 0.95 means it stops at 95% and won't kill the player).")]
    [SerializeField] private float safeAnxietyThreshold = 0.95f;

    [Header("Anxiety Cooldown Settings")]
    [Tooltip("How many seconds the player must be completely safe before anxiety starts going down.")]
    [SerializeField] private float decayDelay = 3f;
    [Tooltip("The starting (slow) speed of anxiety decreasing.")]
    [SerializeField] private float minDecayRate = 0.5f;
    [Tooltip("The maximum (fast) speed of anxiety decreasing.")]
    [SerializeField] private float maxDecayRate = 5f;
    [Tooltip("How many seconds it takes for the decay to speed up from min to max rate.")]
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

        if (heartbeatAudio != null)
        {
            heartbeatAudio.volume = 0f;
        }

        if (tinnitusAudio != null)
        {
            tinnitusAudio.volume = 0f;
        }
    }

    private void Update()
    {
        CheckGaze();
        CheckProximity();
        UpdateAnxiety();

        float anxietyPercent = (float)playerStats.Anxiety / (float)playerStats.MaxAnxiety;

        UpdateHeartbeat(anxietyPercent);
        UpdateTinnitus(anxietyPercent);
        UpdateVisualEffects(anxietyPercent);
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

        // ─── VIGNETTE ─────────────────────────────
        if (_vignette != null && _vignette.active)
        {
            float baseIntensity = Mathf.Lerp(0f, vignetteMaxIntensity, t);
            float intensity = baseIntensity + pulse * pulseStrength;

            _vignette.intensity.Override(Mathf.Lerp(_vignette.intensity.value, intensity, Time.deltaTime * 2f));
            _vignette.color.Override(Color.Lerp(Color.black, new Color(0.4f, 0f, 0f), t));
        }

        // ─── BLUR (CHASE ONLY) ────────────────────
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

        // ─── LIFT / GAMMA / GAIN ───
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

    private void UpdateHeartbeat(float anxietyPercent)
    {
        if (heartbeatAudio == null) return;

        // Start heartbeat early at 30%
        float threshold = Mathf.Min(0.3f, safeAnxietyThreshold - 0.1f);

        if (anxietyPercent >= threshold)
        {
            if (!heartbeatAudio.isPlaying) heartbeatAudio.Play();

            // Fades volume from 0.2 to 1.0 between 30% and 50% anxiety
            float volumeT = Mathf.InverseLerp(threshold, 0.5f, anxietyPercent);
            heartbeatAudio.volume = Mathf.Lerp(0.2f, 1f, volumeT);

            // Pitch slowly ramps up from 30% all the way to max anxiety (95%)
            float pitchT = Mathf.InverseLerp(threshold, safeAnxietyThreshold, anxietyPercent);
            heartbeatAudio.pitch = Mathf.Lerp(1f, 2f, pitchT);
        }
        else
        {
            heartbeatAudio.volume = Mathf.MoveTowards(heartbeatAudio.volume, 0f, Time.deltaTime);

            if (heartbeatAudio.volume <= 0f && heartbeatAudio.isPlaying)
                heartbeatAudio.Stop();
        }
    }

    private void UpdateTinnitus(float anxietyPercent)
    {
        if (tinnitusAudio == null) return;

        float threshold = Mathf.Min(0.7f, safeAnxietyThreshold - 0.1f);
        if (anxietyPercent >= threshold)
        {
            if (!tinnitusAudio.isPlaying) tinnitusAudio.Play();

            float t = Mathf.InverseLerp(threshold, safeAnxietyThreshold, anxietyPercent);
            tinnitusAudio.volume = Mathf.Lerp(0f, 0.8f, t);
        }
        else
        {
            tinnitusAudio.volume = Mathf.MoveTowards(tinnitusAudio.volume, 0f, Time.deltaTime * 2f);

            if (tinnitusAudio.volume <= 0f && tinnitusAudio.isPlaying)
                tinnitusAudio.Stop();
        }
    }
}