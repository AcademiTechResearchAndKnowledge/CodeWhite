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
    // ─────────────────────────────────────────
    //  State
    // ─────────────────────────────────────────
    public enum State { Wandering, Chasing, Teleporting, Weeping }

    [Header("State (Read-Only)")]
    [SerializeField] private State currentState = State.Wandering;
    public State CurrentState => currentState;

    // ─────────────────────────────────────────
    //  References
    // ─────────────────────────────────────────
    [Header("References (Auto-Assigned)")]
    [Tooltip("The script will automatically find this at runtime!")]
    public PlayerReferences playerRef;

    [Tooltip("Where she stands and weeps. Assign in Inspector.")]
    public Transform weepLocation;

    [Header("Interaction")]
    [Tooltip("Drag the child 'SubmitHitbox' object here so we can turn it on/off.")]
    public GameObject submitHitbox;

    // ─────────────────────────────────────────
    //  Special State Settings
    // ─────────────────────────────────────────
    [Header("Special State Timing")]
    public float specialStateInterval = 10f;

    [Header("Special State Probabilities")]
    [Tooltip("Percentage chance (0-100) she will Teleport when rolling a special state.")]
    [Range(0f, 100f)] public float teleportChance = 60f;

    [Tooltip("Percentage chance (0-100) she will Weep when rolling a special state.")]
    [Range(0f, 100f)] public float weepChance = 15f;

    // ─────────────────────────────────────────
    //  Teleport Settings
    // ─────────────────────────────────────────
    [Header("Teleport Settings")]
    public float teleportSearchRadius = 10f;
    public int teleportSampleCount = 20;
    public float teleportIdleDuration = 3f;

    // ─────────────────────────────────────────
    //  Weep Settings
    // ─────────────────────────────────────────
    [Header("Weep Settings")]
    public float weepDuration = 20f;

    // ─────────────────────────────────────────
    //  Private — components
    // ─────────────────────────────────────────
    private NavMeshAgent navMeshAgent;
    private WhiteLadyDetection detection;
    private WhiteLadyWander wander;
    private Flashlight flashlight;

    // ─────────────────────────────────────────
    //  Private — timers
    // ─────────────────────────────────────────
    private float specialStateTimer;
    private float teleportIdleTimer;
    private float weepTimer;

    // ─────────────────────────────────────────
    //  SOUND EFFECTS
    // ─────────────────────────────────────────
    [Header("SFX Settings")]
    public AudioSource audioSource;
    public AudioClip teleportSfx;
    public AudioClip chasingSfx;
    public AudioClip weepingSfx;

    [Tooltip("Plays once when she loses the player and stops chasing.")]
    public AudioClip chasingStoppedSfx;

    // ─────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────
    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        detection = GetComponent<WhiteLadyDetection>();
        wander = GetComponent<WhiteLadyWander>();

        if (weepLocation == null) Debug.LogWarning("[WhiteLady] Weep location not assigned.");

        TryFindPlayer();
    }

    void Start()
    {
        // Ensure the interaction hitbox is off when spawning
        if (submitHitbox != null) submitHitbox.SetActive(false);

        ChangeState(State.Wandering);
    }

    void Update()
    {
        if (playerRef == null)
        {
            TryFindPlayer();
            if (playerRef == null) return;
        }

        switch (currentState)
        {
            case State.Wandering: UpdateWandering(); break;
            case State.Chasing: UpdateChasing(); break;
            case State.Teleporting: UpdateTeleporting(); break;
            case State.Weeping: UpdateWeeping(); break;
        }
    }

    private void TryFindPlayer()
    {
        playerRef = FindAnyObjectByType<PlayerReferences>();

        if (playerRef != null)
        {
            flashlight = playerRef.flashlightScript;
            if (flashlight == null)
                Debug.LogWarning("[WhiteLady] Flashlight not found on PlayerReferences.");
        }
    }

    // ─────────────────────────────────────────
    //  State — Update Ticks
    // ─────────────────────────────────────────

    private float visibleTimer = 0f;
    public float detectionDelay = 0.3f;

    void UpdateWandering()
    {
        specialStateTimer += Time.deltaTime;

        if (specialStateTimer >= specialStateInterval)
        {
            specialStateTimer = 0f;
            RollSpecialState();
            return;
        }

        // Radius, Hiding, Sneaking, and X-Ray (Line of Sight) Check
        bool playerVisible = detection.distanceToPlayer <= detection.detectRange
                          && !detection.IsPlayerHiding()
                          && !detection.IsPlayerSneakingSuccessfully()
                          && detection.HasLineOfSight();

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
        if (navMeshAgent.enabled)
            navMeshAgent.destination = playerRef.transform.position;

        if (detection.IsPlayerHiding())
        {
            wander.InvestigateLocation(detection.GetLastKnownPosition());
            ChangeState(State.Wandering);
            return;
        }

        if (detection.distanceToPlayer > detection.loseRange)
        {
            wander.InvestigateLocation(detection.GetLastKnownPosition());
            ChangeState(State.Wandering);
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

        if (weepLocation != null)
        {
            transform.position = weepLocation.position;

            transform.rotation = weepLocation.rotation;
        }

        if (weepTimer >= weepDuration)
            ChangeState(State.Wandering);
    }

    // ─────────────────────────────────────────
    //  State — Transitions
    // ─────────────────────────────────────────

    void ChangeState(State next)
    {
        if (next == currentState) return;

        bool wasChasing = (currentState == State.Chasing);
        currentState = next;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }

        // Toggle the interaction hitbox based on Weeping state
        if (submitHitbox != null)
        {
            submitHitbox.SetActive(currentState == State.Weeping);
        }

        switch (next)
        {
            case State.Wandering: OnEnterWandering(); break;
            case State.Chasing: OnEnterChasing(); break;
            case State.Teleporting: OnEnterTeleporting(); break;
            case State.Weeping: OnEnterWeeping(); break;
        }

        // Play the "Chase Stopped" stinger if transitioning away from Chasing
        if (wasChasing && audioSource != null && chasingStoppedSfx != null)
        {
            audioSource.PlayOneShot(chasingStoppedSfx);
        }
    }

    void OnEnterWandering()
    {
        SetNav(enabled: true);
        wander.enabled = true;
    }

    void OnEnterChasing()
    {
        SetNav(enabled: true);
        wander.enabled = false;

        audioSource.clip = chasingSfx;
        audioSource.loop = true;
        audioSource.Play();
    }

    void OnEnterTeleporting()
    {
        teleportIdleTimer = 0f;
        wander.enabled = false;
        SetNav(enabled: true);
        navMeshAgent.Warp(FindTeleportPoint());
        flashlight?.Flicker();

        if (teleportSfx != null) audioSource.PlayOneShot(teleportSfx);
    }

    void OnEnterWeeping()
    {
        weepTimer = 0f;
        wander.enabled = false;
        SetNav(enabled: false);

        if (weepLocation != null)
        {
            transform.position = weepLocation.position;
        }

        audioSource.clip = weepingSfx;
        audioSource.loop = true;
        audioSource.Play();
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────

    Vector3 FindTeleportPoint()
    {
        Vector3 best = transform.position;
        float bestDistance = -1f;

        for (int i = 0; i < teleportSampleCount; i++)
        {
            Vector3 candidate = playerRef.transform.position + Random.insideUnitSphere * teleportSearchRadius;
            candidate.y = playerRef.transform.position.y;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, teleportSearchRadius, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(playerRef.transform.position, hit.position);
                if (dist > bestDistance) { bestDistance = dist; best = hit.position; }
            }
        }

        return best;
    }

    void SetNav(bool enabled)
    {
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

        if (roll < teleportChance)
        {
            ChangeState(State.Teleporting);
        }
        else if (roll < (teleportChance + weepChance))
        {
            ChangeState(State.Weeping);
        }
    }
}