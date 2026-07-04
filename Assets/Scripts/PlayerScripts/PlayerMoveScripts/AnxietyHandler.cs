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
    [Tooltip("The maximum distance the player can be before feeling proximity anxiety.")]
    [SerializeField] private float proximityRadius = 10f;

    [Header("Proximity Floor Settings")]
    [Tooltip("The absolute amount of anxiety applied when standing exactly 0 units away from the entity.")]
    [SerializeField] private float maxProximityAnxiety = 50f;

    [Header("Lingering Anxiety Settings (Gaze & Chase)")]
    [SerializeField] private float gazeAnxietyRate = 5f;
    [SerializeField] private float maxGazeAnxiety = 50f;
    [SerializeField] private float chaseAnxietyRate = 4f;
    [SerializeField] private bool isBeingChased = false;

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

    public float externalProximityFloor = 0f;

    private Vignette _vignette;
    private DepthOfField _dof;
    private LiftGammaGain _color;

    private bool isLookingAtAnxietyObject = false;
    private bool isNearAnxietyObject = false;

    private float safeTimer = 0f;
    private bool isFadingOutAudio = false;
    private bool hasMaxedOutAudio = false;

    private float currentProximityDistance = 0f;

    //private float previousActiveFloor = 0f;

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

        if (anxietyPercent >= 1f && !isFadingOutAudio && !hasMaxedOutAudio)
        {
            hasMaxedOutAudio = true;
            StartCoroutine(FadeOutAllAudio());
        }
        else if (anxietyPercent <= safeAnxietyThreshold)
        {
            hasMaxedOutAudio = false;
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
            {
                // MODIFIED: Check if the looked-at object is a weeping White Lady
                WhiteLady whiteLady = hit.collider.GetComponentInParent<WhiteLady>();
                if (whiteLady != null && whiteLady.CurrentState == WhiteLady.State.Weeping)
                {
                    return; // Ignore her entirely, keeping gaze anxiety at false
                }

                isLookingAtAnxietyObject = true;
            }
        }
    }

    private void CheckProximity()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, proximityRadius, anxietyLayerMask);

        // MODIFIED: Filter out the White Lady if she is weeping
        int validAnxietyObjectsCount = 0;
        float closestDist = proximityRadius;

        foreach (Collider col in hitColliders)
        {
            WhiteLady whiteLady = col.GetComponentInParent<WhiteLady>();
            if (whiteLady != null && whiteLady.CurrentState == WhiteLady.State.Weeping)
            {
                continue; // Skip her proximity check entirely while she cries
            }

            validAnxietyObjectsCount++;
            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
            }
        }

        isNearAnxietyObject = validAnxietyObjectsCount > 0;

        if (isNearAnxietyObject)
        {
            currentProximityDistance = closestDist;
        }
        else
        {
            currentProximityDistance = proximityRadius;
        }
    }

    private void UpdateAnxiety()
    {
        float localFloor = 0f;
        if (isNearAnxietyObject)
        {
            float distancePercent = Mathf.Clamp01(1f - (currentProximityDistance / proximityRadius));
            localFloor = distancePercent * maxProximityAnxiety;
        }

        float activeFloor = Mathf.Max(localFloor, externalProximityFloor);
        float currentTotalAnxiety = playerStats.Anxiety;

        if (currentTotalAnxiety < activeFloor)
        {
            playerStats.AddStat(StatType.ANX, activeFloor - currentTotalAnxiety);
            currentTotalAnxiety = playerStats.Anxiety;
        }

        bool isLingeringTriggered = isLookingAtAnxietyObject || isBeingChased;

        if (isLingeringTriggered)
        {
            safeTimer = 0f;

            float currentCap = isBeingChased ? playerStats.MaxAnxiety : maxGazeAnxiety;

            if (currentTotalAnxiety < currentCap)
            {
                float rateToApply = isBeingChased ? chaseAnxietyRate : gazeAnxietyRate;
                float amountToAdd = rateToApply * Time.deltaTime;

                if (currentTotalAnxiety + amountToAdd > currentCap)
                {
                    amountToAdd = currentCap - currentTotalAnxiety;
                }

                playerStats.AddStat(StatType.ANX, amountToAdd);
            }
        }
        else
        {
            if (currentTotalAnxiety > activeFloor + 0.01f)
            {
                safeTimer += Time.deltaTime;

                if (safeTimer >= decayDelay)
                {
                    float timeDecaying = safeTimer - decayDelay;
                    float accelerationProgress = Mathf.Clamp01(timeDecaying / decayAccelerationTime);
                    float currentDecayRate = Mathf.Lerp(minDecayRate, maxDecayRate, accelerationProgress);

                    float maxDecayAllowed = currentTotalAnxiety - activeFloor;
                    float actualDecay = Mathf.Min(currentDecayRate * Time.deltaTime, maxDecayAllowed);

                    playerStats.SubtractStat(StatType.ANX, actualDecay);
                }
            }
        }
    }

    public void ResetSafeTimer()
    {
        safeTimer = 0f;
    }

    public void AddAnxiety(float amount)
    {
        if (playerStats != null)
        {
            playerStats.AddStat(StatType.ANX, amount);
            safeTimer = 0f;
        }
        else
        {
            Debug.LogWarning("AnxietyHandler: Cannot add anxiety because PlayerStats reference is missing.");
        }
    }
}