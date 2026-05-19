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

    [Header("Anxiety Proximity (Spatial)")]
    public float anxietyAuraRadius = 15f;
    [Tooltip("Graph: X-Axis is Distance, Y-Axis is Total Anxiety applied. Set X=0, Y=50 (close) and X=10, Y=10 (far).")]
    public AnimationCurve anxietyDistanceCurve = new AnimationCurve(
      new Keyframe(0f, 50f),
      new Keyframe(5f, 25f),
      new Keyframe(10f, 10f),
      new Keyframe(15f, 0f)
    );
    private float highestAnxietyApplied = 0f;
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

    // --- TRACKING VARIABLES ---
    private bool isCurrentlyIgnoringHiddenPlayer = false;
    private bool isWaitingToStopChaseMusic = false; // Added fake chase audio tracker

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
        isCurrentlyIgnoringHiddenPlayer = false;
        isWaitingToStopChaseMusic = false;
        ChangeState(State.Wandering);
    }

    void Update()
    {
        if (playerRef == null)
        {
            TryFindPlayer();
            if (playerRef == null) return;
        }

        UpdateAnxietyAura();

        // ─── TABLE MECHANIC LOGIC ────────────────────────────────────────────────
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
                        isWaitingToStopChaseMusic = false; // Cancel fake chase if she sees you
                        ChangeState(State.Chasing);
                    }
                }
                else
                {
                    isCurrentlyIgnoringHiddenPlayer = true;

                    if (currentState == State.Chasing)
                    {
                        isWaitingToStopChaseMusic = true; // Trigger the fake chase audio hold
                        ChangeState(State.Investigating);
                    }
                }
            }
        }
        else
        {
            isCurrentlyIgnoringHiddenPlayer = false;
        }
        // ─────────────────────────────────────────────────────────────────────────

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
            playerStats = playerRef.GetComponent<PlayerStats>();
            anxietyHandler = playerRef.GetComponent<AnxietyHandler>();
        }
    }

    private void UpdateAnxietyAura()
    {
        if (playerStats == null || currentState == State.Teleporting) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerRef.transform.position);

        if (distanceToPlayer <= anxietyAuraRadius)
        {
            float targetAnxiety = anxietyDistanceCurve.Evaluate(distanceToPlayer);

            if (targetAnxiety > highestAnxietyApplied)
            {
                float amountToAdd = targetAnxiety - highestAnxietyApplied;
                playerStats.AddStat(StatType.ANX, amountToAdd);
                highestAnxietyApplied = targetAnxiety;

                if (anxietyHandler != null)
                    anxietyHandler.ResetSafeTimer();
            }
        }
        else
        {
            highestAnxietyApplied = 0f;
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
                 && !isCurrentlyIgnoringHiddenPlayer
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

        if (detection.distanceToPlayer > detection.loseRange)
        {
            ChangeState(State.Investigating);
        }
    }

    void UpdateInvestigating()
    {
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
        bool wasFakeChasing = (currentState == State.Investigating && isWaitingToStopChaseMusic);

        currentState = next;

        // Keep the chase music going if we are tricking the player, OR if we re-spotted them during the fake chase
        bool keepChaseMusic = (wasChasing && next == State.Investigating && isWaitingToStopChaseMusic)
             || (wasFakeChasing && next == State.Chasing);

        if (!keepChaseMusic && audioSource != null)
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

        // Play the "lost player" SFX if we finally stopped the chase music
        if (!keepChaseMusic && (wasChasing || wasFakeChasing) && audioSource != null && chasingStoppedSfx != null)
        {
            audioSource.PlayOneShot(chasingStoppedSfx);
        }

        // Clear the fake chase flag if we are moving out of the Investigating state
        if (next != State.Investigating)
        {
            isWaitingToStopChaseMusic = false;
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

        // Added check to prevent stuttering if she re-targets during a fake chase
        if (chasingSfx != null && audioSource.clip != chasingSfx)
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
    }

    void OnEnterTeleporting()
    {
        highestAnxietyApplied = 0f;
        teleportIdleTimer = 0f;
        wander.enabled = false;
        SetNav(enabled: true);

        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.Warp(FindTeleportPoint());
            navMeshAgent.ResetPath();
        }

        // ─── SMART FLICKER ROUTING ──────────────────────────────────────────────────
        if (ClosetHidingSystem.ActiveCloset != null && ClosetHidingSystem.ActiveCloset.InsideCloset)
        {
            // Player is in a closet -> Flicker the closet light
            ClosetHideInteract closetInteract = ClosetHidingSystem.ActiveCloset.GetComponent<ClosetHideInteract>();
            if (closetInteract != null)
            {
                closetInteract.TriggerClosetLightFlicker(1.5f);
            }
        }
        else
        {
            // Player is NOT in a closet -> Flicker their personal flashlight
            flashlight?.Flicker();
        }
        // ────────────────────────────────────────────────────────────────────────────

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