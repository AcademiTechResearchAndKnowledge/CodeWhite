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
    [SerializeField] private float anxietyDecayRate = 1f;

    [Header("Chase State")]
    [SerializeField] private bool isBeingChased = false;

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
            if (!heartbeatAudio.isPlaying) heartbeatAudio.Play();
        }

        if (tinnitusAudio != null)
        {
            tinnitusAudio.volume = 0f;
            if (!tinnitusAudio.isPlaying) tinnitusAudio.Play();
        }
    }

    private void Update()
    {
        CheckGaze();
        CheckProximity();
        UpdateAnxiety();

        float anxietyPercent = playerStats.Anxiety / playerStats.MaxAnxiety;

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
        float t = Mathf.InverseLerp(vignetteThreshold, 1f, anxietyPercent);

        float rawPulse = Mathf.Sin(Time.time * (pulseSpeed + anxietyPercent * 4f));
        float smoothPulse = rawPulse * rawPulse;
        float pulse = smoothPulse * t;

        // ─── VIGNETTE ─────────────────────────────
        if (_vignette != null && _vignette.active)
        {
            float baseIntensity = Mathf.Lerp(0f, vignetteMaxIntensity, t);
            float intensity = baseIntensity + pulse * pulseStrength;

            _vignette.intensity.value = Mathf.Lerp(
                _vignette.intensity.value,
                intensity,
                Time.deltaTime * 2f
            );

            _vignette.color.value = Color.Lerp(Color.black, new Color(0.4f, 0f, 0f), t);
        }

        // ─── BLUR (CHASE ONLY) ────────────────────
        if (_dof != null)
        {
            if (isBeingChased)
            {
                float chaseT = Mathf.InverseLerp(vignetteThreshold, 1f, anxietyPercent);

                float targetStart = Mathf.Lerp(0f, 2f, chaseT);
                float targetEnd = Mathf.Lerp(3f, 0.5f, chaseT);
                float targetRadius = Mathf.Lerp(0f, blurMax, chaseT) + pulse * 0.5f;

                _dof.gaussianStart.value = Mathf.Lerp(_dof.gaussianStart.value, targetStart, Time.deltaTime * 2f);
                _dof.gaussianEnd.value = Mathf.Lerp(_dof.gaussianEnd.value, targetEnd, Time.deltaTime * 2f);
                _dof.gaussianMaxRadius.value = Mathf.Lerp(_dof.gaussianMaxRadius.value, targetRadius, Time.deltaTime * 2f);
            }
            else
            {
                _dof.gaussianStart.value = Mathf.Lerp(_dof.gaussianStart.value, 10f, Time.deltaTime * 2f);
                _dof.gaussianEnd.value = Mathf.Lerp(_dof.gaussianEnd.value, 20f, Time.deltaTime * 2f);
                _dof.gaussianMaxRadius.value = Mathf.Lerp(_dof.gaussianMaxRadius.value, 0f, Time.deltaTime * 2f);
            }
        }

        // ─── LIFT / GAMMA / GAIN (ANXIETY ≥ 90 ONLY) ───
        if (_color != null && _color.active)
        {
            if (anxietyPercent >= 0.9f)
            {
                float t90 = Mathf.InverseLerp(0.9f, 1f, anxietyPercent);

                float rawPulse90 = Mathf.Sin(Time.time * (pulseSpeed + anxietyPercent * 4f));
                float smoothPulse90 = rawPulse90 * rawPulse90;
                float pulse90 = smoothPulse90 * t90;

                Vector3 pulseRed = new Vector3(1f, 0.15f, 0.15f) * (pulse90 * redPulseStrength);

                Vector4 lift = new Vector4(-0.02f, -0.02f, -0.02f, 0f);
                Vector4 gamma = new Vector4(1f + pulseRed.x, 1f + pulseRed.y, 1f + pulseRed.z, 0f);
                Vector4 gain = new Vector4(1f + pulseRed.x, 1f + pulseRed.y, 1f + pulseRed.z, 0f);

                _color.lift.value = Vector4.Lerp(_color.lift.value, lift, Time.deltaTime * 2f);
                _color.gamma.value = Vector4.Lerp(_color.gamma.value, gamma, Time.deltaTime * 2f);
                _color.gain.value = Vector4.Lerp(_color.gain.value, gain, Time.deltaTime * 2f);
            }
            else
            {
                Vector4 neutralLift = new Vector4(0f, 0f, 0f, 0f);
                Vector4 neutralGamma = new Vector4(1f, 1f, 1f, 0f);
                Vector4 neutralGain = new Vector4(1f, 1f, 1f, 0f);

                _color.lift.value = Vector4.Lerp(_color.lift.value, neutralLift, Time.deltaTime * 2f);
                _color.gamma.value = Vector4.Lerp(_color.gamma.value, neutralGamma, Time.deltaTime * 2f);
                _color.gain.value = Vector4.Lerp(_color.gain.value, neutralGain, Time.deltaTime * 2f);
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
        if (isLookingAtAnxietyObject)
            playerStats.AddStat(StatType.ANX, gazeAnxietyRate * Time.deltaTime);
        else if (isNearAnxietyObject)
            playerStats.AddStat(StatType.ANX, proximityAnxietyRate * Time.deltaTime);
        else
            playerStats.SubtractStat(StatType.ANX, anxietyDecayRate * Time.deltaTime);
    }

    private void UpdateHeartbeat(float anxietyPercent)
    {
        if (heartbeatAudio == null) return;

        if (anxietyPercent >= 0.5f)
        {
            float t = Mathf.InverseLerp(0.5f, 1f, anxietyPercent);
            heartbeatAudio.volume = Mathf.Lerp(0.2f, 1f, t);
            heartbeatAudio.pitch = Mathf.Lerp(1f, 2f, t);
        }
        else
        {
            heartbeatAudio.volume = Mathf.MoveTowards(heartbeatAudio.volume, 0f, Time.deltaTime);
        }
    }

    private void UpdateTinnitus(float anxietyPercent)
    {
        if (tinnitusAudio == null) return;

        if (anxietyPercent >= 0.7f)
        {
            float t = Mathf.InverseLerp(0.7f, 1f, anxietyPercent);
            tinnitusAudio.volume = Mathf.Lerp(0f, 0.8f, t);
        }
        else
        {
            tinnitusAudio.volume = Mathf.MoveTowards(tinnitusAudio.volume, 0f, Time.deltaTime * 2f);
        }
    }
}