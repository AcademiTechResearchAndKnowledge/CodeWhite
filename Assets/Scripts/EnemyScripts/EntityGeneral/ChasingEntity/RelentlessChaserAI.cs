using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RelentlessChaserAI : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource; // For chase music
    public AudioSource ambientAudioSource; // For random entity noises
    public AudioClip chasingSfx;

    [Header("Ambient Noise Settings")]
    [Tooltip("Add multiple clips for variety. The entity will pick one at random.")]
    public AudioClip[] ambientNoises;
    [Tooltip("Minimum time in seconds between random noises.")]
    public float minNoiseInterval = 4f;
    [Tooltip("Maximum time in seconds between random noises.")]
    public float maxNoiseInterval = 10f;
    [Tooltip("Volume for ambient noises.")]
    [Range(0f, 1f)] public float ambientVolume = 0.8f;

    private float noiseTimer;

    private Transform playerTransform;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            // Set chase music to 2D
            audioSource.spatialBlend = 0f;
        }
        if (ambientAudioSource != null)
        {
            // Keep ambient noises 3D
            ambientAudioSource.spatialBlend = 1f;
        }
    }

    private void Start()
    {
        FindPlayerTarget();
        StartChaseMusic();
        ResetNoiseTimer();
    }

    private void FindPlayerTarget()
    {
        // Prioritize the PlayerFollow target if you use one for center-mass tracking
        GameObject followObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (followObj != null)
        {
            playerTransform = followObj.transform;
        }
        else
        {
            // Fallback to the main player object
            GameObject mainPlayerObj = GameObject.FindGameObjectWithTag("Player");
            if (mainPlayerObj != null)
            {
                playerTransform = mainPlayerObj.transform;
            }
            else
            {
                Debug.LogError("RelentlessChaserAI: Could not find Player or PlayerFollow tags.");
            }
        }
    }

    private void StartChaseMusic()
    {
        if (audioSource != null && chasingSfx != null)
        {
            audioSource.clip = chasingSfx;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        HandleAmbientNoises();

        // Unrelentingly track the player's exact position every frame
        if (playerTransform != null && navMeshAgent.enabled && !navMeshAgent.isStopped)
        {
            navMeshAgent.destination = playerTransform.position;
        }
    }

    private void HandleAmbientNoises()
    {
        if (ambientNoises != null && ambientNoises.Length > 0)
        {
            noiseTimer -= Time.deltaTime;

            if (noiseTimer <= 0f)
            {
                PlayRandomAmbientNoise();
                ResetNoiseTimer();
            }
        }
    }

    private void PlayRandomAmbientNoise()
    {
        if (ambientAudioSource != null && !ambientAudioSource.isPlaying)
        {
            AudioClip randomClip = ambientNoises[Random.Range(0, ambientNoises.Length)];
            ambientAudioSource.clip = randomClip;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.Play();
        }
    }

    private void ResetNoiseTimer()
    {
        noiseTimer = Random.Range(minNoiseInterval, maxNoiseInterval);
    }
}