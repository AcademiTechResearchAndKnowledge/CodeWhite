using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// White Lady exclusive wandering module.
/// Handles all movement when she is NOT actively chasing the player:
///   Normal      → random patrol
///   Investigating → walk to last known player position
///   LocalSearch → sweep the area after arriving
///   Relocating  → retreat far away to give the player breathing room
/// </summary>
public class WhiteLadyWander : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Sub-State
    // ─────────────────────────────────────────
    public enum WanderState { Normal, Investigating, LocalSearch, Relocating }

    [Header("Wander State (Read-Only)")]
    public WanderState currentState = WanderState.Normal;

    // ─────────────────────────────────────────
    //  Normal Patrol Settings
    // ─────────────────────────────────────────
    [Header("Normal Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolInterval = 5f;

    // ─────────────────────────────────────────
    //  Post-Chase Settings
    // ─────────────────────────────────────────
    [Header("After Losing Player")]
    [Tooltip("How long she searches the area before giving up.")]
    [SerializeField] private float localSearchDuration = 10f;

    [Tooltip("How far she retreats after giving up the search.")]
    [SerializeField] private float retreatDistance = 30f;

    // ─────────────────────────────────────────
    //  Private
    // ─────────────────────────────────────────
    private NavMeshAgent navMeshAgent;
    private float        patrolTimer;
    private float        searchTimer;

    // ─────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────
    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Reset sub-state whenever the White Lady disables this module (e.g. starts chasing)
    void OnDisable()
    {
        currentState = WanderState.Normal;
    }

    void Update()
    {
        switch (currentState)
        {
            case WanderState.Normal:
                TickPatrol(patrolRadius);
                break;

            case WanderState.Investigating:
                if (HasReachedDestination())
                {
                    // Arrived at last known position — begin area search
                    currentState = WanderState.LocalSearch;
                    searchTimer  = 0f;
                    patrolTimer  = patrolInterval; // force immediate first patrol step
                }
                break;

            case WanderState.LocalSearch:
                searchTimer += Time.deltaTime;

                if (searchTimer >= localSearchDuration)
                    Retreat();
                else
                    TickPatrol(patrolRadius);
                break;

            case WanderState.Relocating:
                if (HasReachedDestination())
                    currentState = WanderState.Normal;
                break;
        }
    }

    // ─────────────────────────────────────────
    //  Public API — called by WhiteLady.cs
    // ─────────────────────────────────────────

    /// <summary>
    /// Sends her to investigate a position (player's last known location).
    /// Automatically switches her into Investigating sub-state.
    /// </summary>
    public void InvestigateLocation(Vector3 target)
    {
        if (navMeshAgent == null || !navMeshAgent.enabled) return;

        currentState = WanderState.Investigating;
        navMeshAgent.SetDestination(target);
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────

    void TickPatrol(float radius)
    {
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolInterval)
        {
            patrolTimer = 0f;
            Vector3 nextPos = SampleRandomNavPoint(transform.position, radius);
            navMeshAgent.SetDestination(nextPos);
        }
    }

    void Retreat()
    {
        currentState = WanderState.Relocating;

        Vector3 randomDir = Random.insideUnitSphere.normalized * retreatDistance
                          + transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, retreatDistance * 0.5f, NavMesh.AllAreas))
            navMeshAgent.SetDestination(hit.position);
        else
            currentState = WanderState.Normal; // Failsafe: map too small, just resume patrol
    }

    bool HasReachedDestination()
    {
        return !navMeshAgent.pathPending
            && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    static Vector3 SampleRandomNavPoint(Vector3 origin, float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius + origin;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, radius, NavMesh.AllAreas))
            return hit.position;

        return origin; // Fallback: stay in place this tick
    }
}
