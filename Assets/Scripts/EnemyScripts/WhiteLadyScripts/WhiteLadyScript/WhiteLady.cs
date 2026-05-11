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

    private NavMeshAgent navMeshAgent;
    private WhiteLadyDetection detection;
    private WhiteLadyWander wander;
    private Flashlight flashlight;

    private float specialStateTimer;
    private float teleportIdleTimer;
    private float weepTimer;

    [Header("SFX Settings")]
    public AudioSource audioSource;
    public AudioClip teleportSfx;
    public AudioClip chasingSfx;
    public AudioClip weepingSfx;
    public AudioClip chasingStoppedSfx;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        detection = GetComponent<WhiteLadyDetection>();
        wander = GetComponent<WhiteLadyWander>();

        TryFindPlayer();
    }

    void Start()
    {
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
            case State.Investigating: UpdateInvestigating(); break;
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
        }
    }

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

        if (detection.IsPlayerHiding() || detection.distanceToPlayer > detection.loseRange)
        {
            ChangeState(State.Investigating);
        }
    }

    // ─── FIXED: NO MORE PREMATURE TELEPORTING ───
    void UpdateInvestigating()
    {
        // We completely removed the internal master timer.
        // We now purely wait for the Wander module to report that its local sweep is 100% finished.
        if (wander != null && wander.HasFinishedLocalSearch)
        {
            Debug.Log("[WhiteLady] Local sweep confirmed finished by Wander module. Initiating Teleport.");
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
        if (weepLocation != null)
        {
            transform.position = weepLocation.position;
            transform.rotation = weepLocation.rotation;
        }

        if (weepTimer >= weepDuration)
            ChangeState(State.Wandering);
    }

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

        if (submitHitbox != null) submitHitbox.SetActive(currentState == State.Weeping);

        switch (next)
        {
            case State.Wandering: OnEnterWandering(); break;
            case State.Chasing: OnEnterChasing(); break;
            case State.Investigating: OnEnterInvestigating(); break;
            case State.Teleporting: OnEnterTeleporting(); break;
            case State.Weeping: OnEnterWeeping(); break;
        }

        if (wasChasing && audioSource != null && chasingStoppedSfx != null)
        {
            audioSource.PlayOneShot(chasingStoppedSfx);
        }
    }

    void OnEnterWandering()
    {
        SetNav(enabled: true);
        wander.enabled = true;
        wander.currentState = WhiteLadyWander.WanderState.Normal;
    }

    void OnEnterChasing()
    {
        SetNav(enabled: true);
        wander.enabled = false;

        if (chasingSfx != null)
        {
            audioSource.clip = chasingSfx;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void OnEnterInvestigating()
    {
        SetNav(enabled: true);
        wander.enabled = true;

        wander.InvestigateLocation(detection.GetLastKnownPosition());

        Debug.Log("[WhiteLady] Lost player. Delegating local sweep entirely to Wander module.");
    }

    void OnEnterTeleporting()
    {
        teleportIdleTimer = 0f;
        wander.enabled = false;
        SetNav(enabled: true);

        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.Warp(FindTeleportPoint());
            navMeshAgent.ResetPath();
        }

        flashlight?.Flicker();
        if (teleportSfx != null) audioSource.PlayOneShot(teleportSfx);
    }

    void OnEnterWeeping()
    {
        weepTimer = 0f;
        wander.enabled = false;
        SetNav(enabled: false);

        if (weepLocation != null) transform.position = weepLocation.position;

        if (weepingSfx != null)
        {
            audioSource.clip = weepingSfx;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

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
        if (roll < teleportChance) ChangeState(State.Teleporting);
        else if (roll < (teleportChance + weepChance)) ChangeState(State.Weeping);
    }
}