using UnityEngine;
using UnityEngine.AI;

public class WhiteLadyScript : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  State
    // ─────────────────────────────────────────
    public enum EntityState { Wandering, Chasing, Teleporting, Weeping }

    [Header("State (Read-Only)")]
    [SerializeField] private EntityState currentState = EntityState.Wandering;
    public EntityState CurrentState => currentState;

    // ─────────────────────────────────────────
    //  References
    // ─────────────────────────────────────────
    [Header("References")]
    public Transform player;
    public ClosetHidingSystem closetHidingSystem;
    public Transform weepLocation;

    // ─────────────────────────────────────────
    //  Detection Settings
    // ─────────────────────────────────────────
    [Header("Detection Settings")]
    public float detectRange = 6f; // Updated per your settings
    public float hideAllowedRange = 12f;
    public float loseRange = 10f; // Updated per your settings

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
    //  Special State Roll
    // ─────────────────────────────────────────
    [Header("Special State Timer")]
    public float specialStateInterval = 10f; // Updated per your settings

    // ─────────────────────────────────────────
    //  Public Info
    // ─────────────────────────────────────────
    [Header("Info (Read-Only)")]
    public bool canHideFromEnemy;
    public float distanceToPlayer;

    // ─────────────────────────────────────────
    //  Private — timers & components
    // ─────────────────────────────────────────
    private float specialStateTimer = 0f;
    private float teleportIdleTimer = 0f;
    private float weepTimer = 0f;

    private EntityAi entityAi;
    private EntityWondering entityWondering;
    private NavMeshAgent navMeshAgent;
    private Flashlight flashlight;

    void Awake()
    {
        entityAi = GetComponent<EntityAi>();
        entityWondering = GetComponent<EntityWondering>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        flashlight = player.GetComponent<Flashlight>();

        if (flashlight == null)
            Debug.LogWarning("[Entity] Flashlight script not found on player!");
        if (weepLocation == null)
            Debug.LogWarning("[Entity] Weep location not set in Inspector!");
    }

    void Start()
    {
        ChangeState(EntityState.Wandering);
    }

    void Update()
    {
        if (player == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        canHideFromEnemy = distanceToPlayer > hideAllowedRange;

        switch (currentState)
        {
            case EntityState.Wandering: UpdateWandering(); break;
            case EntityState.Chasing: UpdateChasing(); break;
            case EntityState.Teleporting: UpdateTeleporting(); break;
            case EntityState.Weeping: UpdateWeeping(); break;
        }
    }

    void UpdateWandering()
    {
        specialStateTimer += Time.deltaTime;

        if (specialStateTimer >= specialStateInterval)
        {
            specialStateTimer = 0f;
            RollSpecialState();
            return;
        }

        if (distanceToPlayer <= detectRange)
        {
            ChangeState(EntityState.Chasing);
        }
    }

    void UpdateChasing()
    {
        if (closetHidingSystem != null && closetHidingSystem.InsideCloset)
        {
            ChangeState(EntityState.Wandering);
            return;
        }

        if (distanceToPlayer > loseRange)
        {
            ChangeState(EntityState.Wandering);
        }
    }

    void UpdateTeleporting()
    {
        teleportIdleTimer += Time.deltaTime;

        if (teleportIdleTimer >= teleportIdleDuration)
            ChangeState(EntityState.Wandering);
    }

    void UpdateWeeping()
    {
        weepTimer += Time.deltaTime;

        // Ensure the entity remains exactly at the weep location every frame
        if (weepLocation != null)
        {
            transform.position = weepLocation.position;
            transform.rotation = weepLocation.rotation;
        }

        if (weepTimer >= weepDuration)
            ChangeState(EntityState.Wandering);
    }

    void ChangeState(EntityState newState)
    {
        if (newState == currentState) return;

        currentState = newState;

        switch (newState)
        {
            case EntityState.Wandering: OnEnterWandering(); break;
            case EntityState.Chasing: OnEnterChasing(); break;
            case EntityState.Teleporting: OnEnterTeleporting(); break;
            case EntityState.Weeping: OnEnterWeeping(); break;
        }
    }

    void OnEnterWandering()
    {
        specialStateTimer = 0f;
        navMeshAgent.enabled = true; // Re-enable to allow movement
        navMeshAgent.isStopped = false;
        entityAi.enabled = false;
        entityWondering.enabled = true;
    }

    void OnEnterChasing()
    {
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        entityAi.enabled = true;
        entityWondering.enabled = false;
    }

    void OnEnterTeleporting()
    {
        teleportIdleTimer = 0f;
        entityAi.enabled = false;
        entityWondering.enabled = false;

        Vector3 bestPoint = transform.position;
        float bestDistance = -1f;

        for (int i = 0; i < teleportSampleCount; i++)
        {
            Vector3 candidate = player.position + Random.insideUnitSphere * teleportSearchRadius;
            candidate.y = player.position.y;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, teleportSearchRadius, NavMesh.AllAreas))
            {
                float dist = Vector3.Distance(player.position, hit.position);
                if (dist > bestDistance)
                {
                    bestDistance = dist;
                    bestPoint = hit.position;
                }
            }
        }

        navMeshAgent.Warp(bestPoint);
        if (flashlight != null) flashlight.Flicker();
    }

    void OnEnterWeeping()
    {
        weepTimer = 0f;
        entityAi.enabled = false;
        entityWondering.enabled = false;
        navMeshAgent.enabled = false; // fully stop — no drift

        if (weepLocation != null)
        {
            transform.position = weepLocation.position;
            transform.rotation = weepLocation.rotation;
        }
        else
        {
            Debug.LogWarning("[Entity] Weep location not set — weeping in place.");
        }
    }

    void RollSpecialState()
    {
        int roll = Random.Range(0, 4);

        switch (roll)
        {
            case 0: ChangeState(EntityState.Teleporting); break;
            case 1: ChangeState(EntityState.Weeping); break;
            default: break;
        }
    }
}