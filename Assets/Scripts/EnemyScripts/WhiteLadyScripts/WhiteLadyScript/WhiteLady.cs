using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// White Lady — master state machine.
/// Single source of truth for all her behaviour.
/// Delegates movement to WhiteLadyWander, detection queries to WhiteLadyDetection.
///
///  States:
///    Wandering   → patrolling; may roll a special state on a timer
///    Chasing     → pursuing the player directly
///    Teleporting → warps to a random position; flickers the flashlight
///    Weeping     → frozen at a set location for a duration
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
    [Header("References")]
    [Tooltip("The player's Transform. Assign in Inspector.")]
    public Transform player;

    [Tooltip("Where she stands and weeps. Assign in Inspector.")]
    public Transform weepLocation;

    // ─────────────────────────────────────────
    //  Special State Settings
    // ─────────────────────────────────────────
    [Header("Special State Roll")]
    [Tooltip("Seconds between special state rolls while wandering.")]
    public float specialStateInterval = 10f;

    // ─────────────────────────────────────────
    //  Teleport Settings
    // ─────────────────────────────────────────
    [Header("Teleport Settings")]
    [Tooltip("Radius around the player to sample teleport candidates from.")]
    public float teleportSearchRadius = 10f;

    [Tooltip("How many NavMesh positions to sample when choosing a teleport point.")]
    public int   teleportSampleCount  = 20;

    [Tooltip("How long she idles at the teleport destination before returning to Wandering.")]
    public float teleportIdleDuration = 3f;

    // ─────────────────────────────────────────
    //  Weep Settings
    // ─────────────────────────────────────────
    [Header("Weep Settings")]
    [Tooltip("How long she weeps before returning to Wandering.")]
    public float weepDuration = 20f;

    // ─────────────────────────────────────────
    //  Private — components
    // ─────────────────────────────────────────
    private NavMeshAgent      navMeshAgent;
    private WhiteLadyDetection detection;
    private WhiteLadyWander    wander;
    private Flashlight         flashlight;

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

    // ─────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────
    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        detection    = GetComponent<WhiteLadyDetection>();
        wander       = GetComponent<WhiteLadyWander>();

        if (player != null)
            flashlight = player.GetComponent<Flashlight>();

        if (flashlight   == null) Debug.LogWarning("[WhiteLady] Flashlight not found on player.");
        if (weepLocation == null) Debug.LogWarning("[WhiteLady] Weep location not assigned.");
    }

    void Start()
    {
        ChangeState(State.Wandering);
    }

    void Update()
    {
        if (player == null) return;

        switch (currentState)
        {
            case State.Wandering:   UpdateWandering();   break;
            case State.Chasing:     UpdateChasing();     break;
            case State.Teleporting: UpdateTeleporting(); break;
            case State.Weeping:     UpdateWeeping();     break;
        }
    }

    // ─────────────────────────────────────────
    //  State — Update Ticks
    // ─────────────────────────────────────────

    private float visibleTimer = 0f;
    public float detectionDelay = 0.3f; // tweak (0.2–0.5)

    void UpdateWandering()
    {
        specialStateTimer += Time.deltaTime;

        if (specialStateTimer >= specialStateInterval)
        {
            specialStateTimer = 0f;
            RollSpecialState();
            return;
        }

        bool playerVisible = detection.distanceToPlayer <= detection.detectRange
                          && !detection.IsPlayerHiding()
                          && !detection.IsPlayerSneakingSuccessfully();

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
        // Drive the NavMeshAgent toward the player every frame
        if (navMeshAgent.enabled)
            navMeshAgent.destination = player.position;

        // Player hid — investigate last known position then wander
        if (detection.IsPlayerHiding()) // <--- If this is true even for one frame, audio stops.
        {
            wander.InvestigateLocation(detection.GetLastKnownPosition());
            ChangeState(State.Wandering);
            return;
        }

        // Player ran too far — same: investigate then wander
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

        // Keep her locked to the weep anchor (NavMesh-safe)
        if (weepLocation != null)
        {
            navMeshAgent.Warp(weepLocation.position);
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
        currentState = next;

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false; // Turn off looping so it doesn't "stick"
        }

      

        switch (next)
        {
            case State.Wandering:   OnEnterWandering();   break;
            case State.Chasing:     OnEnterChasing();     break;
            case State.Teleporting: OnEnterTeleporting(); break;
            case State.Weeping:     OnEnterWeeping();     break;
        }
    }

    void OnEnterWandering()
    {
        SetNav(enabled: true);
        wander.enabled = true;

        audioSource.Stop();
    }

    void OnEnterChasing()
    {
        SetNav(enabled: true);
        wander.enabled = false;

        audioSource.clip = chasingSfx;
        audioSource.loop = true; // Loop the chase sound
        audioSource.Play();
    }

    void OnEnterTeleporting()
    {
        teleportIdleTimer  = 0f;
        wander.enabled     = false;
        SetNav(enabled: true);
        navMeshAgent.Warp(FindTeleportPoint());
        flashlight?.Flicker();

        if (teleportSfx != null) audioSource.PlayOneShot(teleportSfx);

    }

    void OnEnterWeeping()
    {
        weepTimer      = 0f;
        wander.enabled = false;
        SetNav(enabled: false); // fully stop — no drift

        if (weepLocation != null)
        {
            transform.position = weepLocation.position;
        }
        else
        {
            Debug.LogWarning("[WhiteLady] Weep location not set — weeping in place.");
        }

        audioSource.clip = weepingSfx;
        audioSource.loop = true;
        audioSource.Play();
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────

    /// <summary>
    /// Samples <see cref="teleportSampleCount"/> random NavMesh points around the player
    /// and returns the one farthest from the player within <see cref="teleportSearchRadius"/>.
    /// </summary>
    Vector3 FindTeleportPoint()
    {
        Vector3 best         = transform.position;
        float   bestDistance = -1f;

        for (int i = 0; i < teleportSampleCount; i++)
        {
            Vector3 candidate   = player.position + Random.insideUnitSphere * teleportSearchRadius;
            candidate.y         = player.position.y;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, teleportSearchRadius, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(player.position, hit.position);
                if (dist > bestDistance) { bestDistance = dist; best = hit.position; }
            }
        }

        return best;
    }

    /// <summary>Enables or disables the NavMeshAgent cleanly.</summary>
    void SetNav(bool enabled)
    {
        if (enabled)
        {
            navMeshAgent.enabled = true;

            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = false;
        }
        else
        {
            if (navMeshAgent.isOnNavMesh)
                navMeshAgent.isStopped = true;

            navMeshAgent.enabled = false;
        }
    }

    /// <summary>50% chance to enter a special state; otherwise stays Wandering.</summary>
    void RollSpecialState()
    {
        switch (Random.Range(0, 4))
        {
            case 0: ChangeState(State.Teleporting); break;
            case 1: ChangeState(State.Weeping);     break;
            // cases 2 & 3: no-op, keep wandering
        }
    }
}
