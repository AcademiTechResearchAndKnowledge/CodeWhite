using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Required for Vignette

public class AnxietyHandler : MonoBehaviour
{
    // ─── References ────────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Volume globalVolume;

    [Header("Audio")]
    [SerializeField] private AudioSource heartbeatAudio;
    [SerializeField] private AudioSource tinnitusAudio;

    // ─── Anxiety Trigger Settings ───────────────────────────────────────────────
    [Header("Anxiety Object Settings")]
    [Tooltip("Select the layer(s) that should trigger anxiety.")]
    [SerializeField] private LayerMask anxietyLayerMask; // Replaced Tag with LayerMask
    [SerializeField] private float gazeDetectionRange = 20f;
    [SerializeField] private float proximityRadius = 5f;
    [SerializeField] private float gazeAnxietyRate = 5f;
    [SerializeField] private float proximityAnxietyRate = 2f;
    [SerializeField] private float anxietyDecayRate = 1f;

    // ─── Heartbeat Settings ─────────────────────────────────────────────────────
    [Header("Heartbeat Settings")]
    [SerializeField] private float heartbeatThreshold = 0.5f;
    [SerializeField] private float heartbeatMinPitch = 1.0f;
    [SerializeField] private float heartbeatMaxPitch = 2.0f;
    [SerializeField] private float heartbeatMinVolume = 0.2f;
    [SerializeField] private float heartbeatMaxVolume = 1.0f;

    // ─── Tinnitus Settings ──────────────────────────────────────────────────────
    [Header("Tinnitus Settings")]
    [SerializeField] private float tinnitusThreshold = 0.7f;
    [SerializeField] private float tinnitusMinVolume = 0f;
    [SerializeField] private float tinnitusMaxVolume = 0.8f;

    // ─── Vignette Settings (Post Processing) ────────────────────────────────────
    [Header("Post-Processing Vignette")]
    [Tooltip("Anxiety % (0–1) at which screen darkening begins.")]
    [SerializeField] private float vignetteThreshold = 0.7f;

    [Tooltip("Maximum intensity of the PP Vignette (usually 0 to 1).")]
    [Range(0f, 1f)]
    [SerializeField] private float vignetteMaxIntensity = 0.5f;

    private Vignette _vignette;
    private bool isLookingAtAnxietyObject = false;
    private bool isNearAnxietyObject = false;

    private void Awake()
    {
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (globalVolume != null && globalVolume.profile.TryGet(out Vignette v))
        {
            _vignette = v;
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
        UpdateVignette(anxietyPercent);
    }

    private void CheckGaze()
    {
        isLookingAtAnxietyObject = false;
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, gazeDetectionRange))
        {
            // Bitwise check: Is the object's layer included in our anxietyLayerMask?
            if (((1 << hit.collider.gameObject.layer) & anxietyLayerMask) != 0)
            {
                if (hit.collider.name == "WhiteLady")
                {
                    WhiteLady lady = hit.collider.GetComponent<WhiteLady>();
                    if (lady != null && lady.CurrentState == WhiteLady.State.Weeping)
                    {
                        return;
                    }
                }

                isLookingAtAnxietyObject = true;
            }
        }
    }

    private void CheckProximity()
    {
        isNearAnxietyObject = false;

        // Highly optimized: We pass the anxietyLayerMask directly into the Sphere check. 
        // Unity now completely ignores walls, floors, and other unselected layers.
        Collider[] nearby = Physics.OverlapSphere(transform.position, proximityRadius, anxietyLayerMask);

        foreach (Collider col in nearby)
        {
            if (col.name == "WhiteLady")
            {
                WhiteLady lady = col.GetComponent<WhiteLady>();
                if (lady != null && lady.CurrentState == WhiteLady.State.Weeping)
                {
                    continue; // Skip the weeping lady and check the rest of the array
                }
            }

            // If we find any valid anxiety object, flag it and stop checking
            isNearAnxietyObject = true;
            break;
        }
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
        if (anxietyPercent >= heartbeatThreshold)
        {
            float t = Mathf.InverseLerp(heartbeatThreshold, 1f, anxietyPercent);
            heartbeatAudio.volume = Mathf.Lerp(heartbeatMinVolume, heartbeatMaxVolume, t);
            heartbeatAudio.pitch = Mathf.Lerp(heartbeatMinPitch, heartbeatMaxPitch, t);
        }
        else
        {
            heartbeatAudio.volume = Mathf.MoveTowards(heartbeatAudio.volume, 0f, Time.deltaTime);
            heartbeatAudio.pitch = heartbeatMinPitch;
        }
    }

    private void UpdateTinnitus(float anxietyPercent)
    {
        if (tinnitusAudio == null) return;
        if (anxietyPercent >= tinnitusThreshold)
        {
            float t = Mathf.InverseLerp(tinnitusThreshold, 1f, anxietyPercent);
            tinnitusAudio.volume = Mathf.Lerp(tinnitusMinVolume, tinnitusMaxVolume, t);
        }
        else
        {
            tinnitusAudio.volume = Mathf.MoveTowards(tinnitusAudio.volume, 0f, Time.deltaTime * 2f);
        }
    }

    private void UpdateVignette(float anxietyPercent)
    {
        if (_vignette == null) return;

        float targetIntensity = 0f;

        if (anxietyPercent >= vignetteThreshold)
        {
            float t = Mathf.InverseLerp(vignetteThreshold, 1f, anxietyPercent);
            targetIntensity = Mathf.Lerp(0f, vignetteMaxIntensity, t);
        }

        _vignette.intensity.value = Mathf.MoveTowards(_vignette.intensity.value, targetIntensity, Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        Gizmos.DrawSphere(transform.position, proximityRadius);
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * gazeDetectionRange);
        }
    }
}