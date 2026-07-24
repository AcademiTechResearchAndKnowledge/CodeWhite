using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// White Lady — master state machine.
/// Single source of truth for all her behaviour.
/// Delegates movement to WhiteLadyWander, detection queries to WhiteLadyDetection.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(WhiteLadyDetection))]
[RequireComponent(typeof(WhiteLadyWander))]
public class WhiteLady : MonoBehaviour
{
    public enum State { Wandering, Chasing, Investigating, Teleporting, Weeping }

    [Header("State (Read-Only)")]
    [SerializeField] private State currentState = State.Wandering;
    public State CurrentState => currentState;

    [Header("References")]
    public PlayerReferences playerRef;
    public Transform weepLocation;
    public GameObject submitHitbox;

    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;

    [Header("Special State Timing")]
    public float specialStateInterval = 10f;
    [Range(0f, 100f)] public float teleportChance = 60f;
    [Range(0f, 100f)] public float weepChance = 15f;

    [Header("Teleport Settings")]
    public float teleportSearchRadius = 10f;
    public int teleportSampleCount = 20;
    public float teleportIdleDuration = 3f;

    [Header("Weep Settings")]
    public float weepDuration = 20f;
    [Tooltip("If true, line of sight during Weeping forces her back into Chasing state.")]
    public bool canBeDisturbedWhileWeeping = false;

    private NavMeshAgent navMeshAgent;
    private WhiteLadyDetection detection;
    private WhiteLadyWander wander;
    private Flashlight flashlight;

    private float specialStateTimer;
    private float teleportIdleTimer;
    private float weepTimer;

    [Header("SFX Settings")]
    public AudioSource audioSource;
    public AudioSource weepingAudioSource;
    public AudioClip teleportSfx;
    public AudioClip chasingSfx;
    public AudioClip weepingSfx;
    public AudioClip chasingStoppedSfx;

    [Header("Ambient Noise Settings")]
    public AudioSource ambientAudioSource;
    [Tooltip("Add multiple clips for variety. The entity will pick one at random.")]
    public AudioClip[] ambientNoises;
    [Tooltip("Minimum time in seconds between random noises.")]
    public float minNoiseInterval = 4f;
    [Tooltip("Maximum time in seconds between random noises.")]
    public float maxNoiseInterval = 10f;
    [Tooltip("Volume for ambient noises.")]
    [Range(0f, 1f)] public float ambientVolume = 0.8f;

    [Header("Despawn Effects")]
    [Tooltip("The particle effect to spawn when the entity disappears.")]
    public GameObject despawnParticlePrefab;
    [Tooltip("The sound to play when the entity despawns (e.g., chase end music).")]
    public AudioClip despawnSound;
    [Tooltip("Volume for the despawn sound.")]
    [Range(0f, 1f)] public float despawnVolume = 1.0f;

    private float noiseTimer;

    private bool isCurrentlyIgnoringHiddenPlayer = false;
    private bool isWaitingToStopChaseMusic = false;

    private float visibleTimer = 0f;
    public float detectionDelay = 0.3f;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        detection = GetComponent<WhiteLadyDetection>();
        wander = GetComponent<WhiteLadyWander>();
        TryFindPlayer();

        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;
        }

        if (weepingAudioSource != null)
        {
            weepingAudioSource.spatialBlend = 0f;
            weepingAudioSource.rolloffMode = AudioRolloffMode.Linear;
            weepingAudioSource.minDistance = 5f;
            weepingAudioSource.maxDistance = 30f;
        }

        if (ambientAudioSource != null)
        {
            ambientAudioSource.spatialBlend = 1f;
            ambientAudioSource.rolloffMode = AudioRolloffMode.Linear;
            ambientAudioSource.minDistance = 10f;
            ambientAudioSource.maxDistance = 40f;
        }
    }

    void Start()
    {
        // FIX: Force NavMeshAgent stopping distance low so it doesn't stop outside catchRadius
        if (navMeshAgent != null)
        {
            navMeshAgent.stoppingDistance = 0.5f;
        }

        if (submitHitbox != null) submitHitbox.SetActive(false);
        isCurrentlyIgnoringHiddenPlayer = false;
        isWaitingToStopChaseMusic = false;

        ResetNoiseTimer();
        ChangeState(State.Wandering);
    }

    void Update()
    {
        HandleAmbientNoises();

        if (playerRef == null || playerRef.transform == null)
        {
            TryFindPlayer();
            if (playerRef == null || playerRef.transform == null) return;
        }

        bool isHiding = detection.IsPlayerHiding();

        if (isHiding)
        {
            if (!isCurrentlyIgnoringHiddenPlayer)
            {
                if (detection.distanceToPlayer <= detection.detectRange)
                {
                    if (currentState == State.Wandering || currentState == State.Investigating)
                    {
                        Debug.Log("Player hid too close! White Lady is attacking!");
                        isWaitingToStopChaseMusic = false;
                        ChangeState(State.Chasing);
                    }
                }
                else
                {
                    isCurrentlyIgnoringHiddenPlayer = true;

                    if (currentState == State.Chasing)
                    {
                        isWaitingToStopChaseMusic = true;
                        ChangeState(State.Investigating);
                    }
                }
            }
        }
        else
        {
            isCurrentlyIgnoringHiddenPlayer = false;
        }

        switch (currentState)
        {
            case State.Wandering: UpdateWandering(); break;
            case State.Chasing: UpdateChasing(); break;
            case State.Investigating: UpdateInvestigating(); break;
            case State.Teleporting: UpdateTeleporting(); break;
            case State.Weeping: UpdateWeeping(); break;
        }
    }

    private void HandleAmbientNoises()
    {
        if (currentState == State.Weeping || currentState == State.Chasing) return;

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

    private void TryFindPlayer()
    {
        playerRef = FindAnyObjectByType<PlayerReferences>();
        if (playerRef != null)
        {
            flashlight = playerRef.flashlightScript;
            playerStats = playerRef.GetComponent<PlayerStats>();
            anxietyHandler = playerRef.GetComponent<AnxietyHandler>();
        }
    }

    void UpdateWandering()
    {
        bool playerVisible = detection.distanceToPlayer <= detection.detectRange
                 && !isCurrentlyIgnoringHiddenPlayer
                 && !detection.IsPlayerSneakingSuccessfully()
                 && detection.HasLineOfSight();

        if (!playerVisible)
        {
            specialStateTimer += Time.deltaTime;

            if (specialStateTimer >= specialStateInterval)
            {
                specialStateTimer = 0f;
                RollSpecialState();
                return;
            }
        }

        if (playerVisible)
        {
            visibleTimer += Time.deltaTime;
            if (visibleTimer >= detectionDelay)
                ChangeState(State.Chasing);
        }
        else
        {
            visibleTimer = 0f;
        }
    }

    void UpdateChasing()
    {
        // FIX: Safeguarded playerRef.transform against null reference exceptions
        if (navMeshAgent != null && navMeshAgent.enabled && playerRef != null && playerRef.transform != null)
        {
            navMeshAgent.destination = playerRef.transform.position;
        }

        if (detection.distanceToPlayer > detection.loseRange)
        {
            ChangeState(State.Investigating);
        }
    }

    void UpdateInvestigating()
    {
        bool playerVisible = detection.distanceToPlayer <= detection.detectRange
                 && !isCurrentlyIgnoringHiddenPlayer
                 && !detection.IsPlayerSneakingSuccessfully()
                 && detection.HasLineOfSight();

        if (playerVisible)
        {
            ChangeState(State.Chasing);
            return;
        }

        if (wander != null && wander.HasFinishedLocalSearch)
        {
            ChangeState(State.Teleporting);
        }
    }

    void UpdateTeleporting()
    {
        teleportIdleTimer += Time.deltaTime;
        if (teleportIdleTimer >= teleportIdleDuration)
            ChangeState(State.Wandering);
    }

    void UpdateWeeping()
    {
        weepTimer += Time.deltaTime;

        if (canBeDisturbedWhileWeeping && detection.distanceToPlayer <= detection.detectRange && detection.HasLineOfSight())
        {
            ChangeState(State.Chasing);
            return;
        }

        if (weepTimer >= weepDuration)
            ChangeState(State.Wandering);
    }

    void ChangeState(State next)
    {
        if (next == currentState) return;

        bool wasChasing = (currentState == State.Chasing);
        bool wasFakeChasing = (currentState == State.Investigating && isWaitingToStopChaseMusic);
        bool wasWeeping = (currentState == State.Weeping);

        currentState = next;

        bool keepChaseMusic = (wasChasing && next == State.Investigating && isWaitingToStopChaseMusic)
             || (wasFakeChasing && next == State.Chasing);

        if (!keepChaseMusic && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }

        if (wasWeeping && weepingAudioSource != null)
        {
            weepingAudioSource.Stop();
            weepingAudioSource.clip = null;
            weepingAudioSource.loop = false;
        }

        if (submitHitbox != null) submitHitbox.SetActive(currentState == State.Weeping);

        switch (next)
        {
            case State.Wandering: OnEnterWandering(); break;
            case State.Chasing: OnEnterChasing(); break;
            case State.Investigating: OnEnterInvestigating(); break;
            case State.Teleporting: OnEnterTeleporting(); break;
            case State.Weeping: OnEnterWeeping(); break;
        }

        if (!keepChaseMusic && (wasChasing || wasFakeChasing) && audioSource != null && chasingStoppedSfx != null)
        {
            audioSource.PlayOneShot(chasingStoppedSfx);
        }

        if (next != State.Investigating)
        {
            isWaitingToStopChaseMusic = false;
        }
    }

    void OnEnterWandering()
    {
        SetNav(enabled: true);
        if (wander != null)
        {
            wander.enabled = true;
            wander.currentState = WhiteLadyWander.WanderState.Normal;
        }
    }

    void OnEnterChasing()
    {
        SetNav(enabled: true);
        if (wander != null) wander.enabled = false;

        // FIX: Re-enforce stopping distance override when chasing starts
        if (navMeshAgent != null)
        {
            navMeshAgent.stoppingDistance = 0.5f;
        }

        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }

        if (chasingSfx != null && audioSource != null && audioSource.clip != chasingSfx)
        {
            audioSource.clip = chasingSfx;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void OnEnterInvestigating()
    {
        SetNav(enabled: true);
        if (wander != null)
        {
            wander.enabled = true;
            wander.InvestigateLocation(detection.GetLastKnownPosition());
        }
    }

    void OnEnterTeleporting()
    {
        teleportIdleTimer = 0f;
        if (wander != null) wander.enabled = false;
        SetNav(enabled: true);

        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.Warp(FindTeleportPoint());
            navMeshAgent.ResetPath();
        }

        if (ClosetHidingSystem.ActiveCloset != null && ClosetHidingSystem.ActiveCloset.InsideCloset)
        {
            ClosetHideInteract closetInteract = ClosetHidingSystem.ActiveCloset.GetComponent<ClosetHideInteract>();
            if (closetInteract != null)
            {
                closetInteract.TriggerClosetLightFlicker(1.5f);
            }
        }
        else
        {
            flashlight?.Flicker();
        }

        if (teleportSfx != null && audioSource != null) audioSource.PlayOneShot(teleportSfx);
    }

    void OnEnterWeeping()
    {
        weepTimer = 0f;
        if (wander != null) wander.enabled = false;

        SetNav(enabled: false);

        if (weepLocation != null)
        {
            transform.position = weepLocation.position;
            transform.rotation = weepLocation.rotation;
        }

        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }

        if (weepingSfx != null && weepingAudioSource != null)
        {
            weepingAudioSource.clip = weepingSfx;
            weepingAudioSource.loop = true;
            weepingAudioSource.Play();
        }
    }

    Vector3 FindTeleportPoint()
    {
        Vector3 best = transform.position;
        float bestDistance = -1f;

        if (playerRef == null || playerRef.transform == null) return best;

        for (int i = 0; i < teleportSampleCount; i++)
        {
            Vector2 randomRing = Random.insideUnitCircle * teleportSearchRadius;
            Vector3 candidate = playerRef.transform.position + new Vector3(randomRing.x, 0f, randomRing.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(playerRef.transform.position, hit.position);
                if (dist > bestDistance)
                {
                    bestDistance = dist;
                    best = hit.position;
                }
            }
        }
        return best;
    }

    void SetNav(bool enabled)
    {
        if (navMeshAgent == null) return;

        if (enabled)
        {
            navMeshAgent.enabled = true;
            if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = false;
        }
        else
        {
            if (navMeshAgent.isOnNavMesh) navMeshAgent.isStopped = true;
            navMeshAgent.enabled = false;
        }
    }

    void RollSpecialState()
    {
        float roll = Random.Range(0f, 100f);
        if (roll < teleportChance) ChangeState(State.Teleporting);
        else if (roll < (teleportChance + weepChance)) ChangeState(State.Weeping);
    }

    public void Despawn()
    {
        if (despawnParticlePrefab != null)
        {
            Instantiate(despawnParticlePrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("Despawn Particle Prefab is not assigned on " + gameObject.name);
        }

        if (despawnSound != null)
        {
            Play2DAudio(despawnSound, despawnVolume);
        }

        Destroy(gameObject);
    }

    private void Play2DAudio(AudioClip clip, float volume)
    {
        GameObject tempAudioObject = new GameObject("TempDespawnAudio");
        AudioSource source = tempAudioObject.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;

        source.Play();
        Destroy(tempAudioObject, clip.length);
    }
}