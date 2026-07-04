using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// White Lady exclusive wandering module.
/// Handles standard patrolling AND active localized area sweeping around an anchor point.
/// Controlled entirely by the WhiteLady.cs master brain.
/// </summary>
public class WhiteLadyWander : MonoBehaviour
{
    public enum WanderState { Normal, Investigating, LocalSearch }

    [Header("Current Sub-State (Read-Only)")]
    public WanderState currentState = WanderState.Normal;

    [Header("Normal Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float patrolInterval = 5f;

    [Header("Local Search Settings (Post-Chase)")]
    [Tooltip("How many seconds she spends wandering around your last known spot.")]
    public float localSearchDuration = 10f;

    [Tooltip("How wide she paces around the closet area while searching.")]
    [SerializeField] private float searchRadius = 8f;

    private NavMeshAgent navMeshAgent;
    private float patrolTimer;
    private float searchTimer;

    private Vector3 anchorPoint;
    public bool HasFinishedLocalSearch { get; private set; } = false;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void OnDisable()
    {
        currentState = WanderState.Normal;
        HasFinishedLocalSearch = false;
    }

    void Update()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled) return;

        switch (currentState)
        {
            case WanderState.Normal:
                TickPatrol(transform.position, patrolRadius);
                break;

            case WanderState.Investigating:
                if (HasReachedDestination())
                {
                    currentState = WanderState.LocalSearch;
                    searchTimer = 0f;
                    patrolTimer = patrolInterval; // Force immediate first sweep step

                    if (navMeshAgent.isOnNavMesh) navMeshAgent.ResetPath();

                    Debug.Log("[WhiteLadyWander] Arrived at anchor spot. Commencing local sweep.");
                }
                break;

            case WanderState.LocalSearch:
                searchTimer += Time.deltaTime;

                // 1. FIXED: The timer expired, BUT we require her to finish walking to her current point first!
                if (searchTimer >= localSearchDuration)
                {
                    // This guarantees she never vanishes mid-step again
                    if (HasReachedDestination())
                    {
                        HasFinishedLocalSearch = true;
                    }
                }
                // 2. Otherwise, keep picking new patrol points around the closet anchor
                else
                {
                    TickPatrol(anchorPoint, searchRadius);
                }
                break;
        }
    }

    public void InvestigateLocation(Vector3 target)
    {
        if (navMeshAgent == null || !navMeshAgent.enabled) return;

        currentState = WanderState.Investigating;
        HasFinishedLocalSearch = false;
        searchTimer = 0f;

        anchorPoint = target;
        navMeshAgent.SetDestination(anchorPoint);
    }

    void TickPatrol(Vector3 centerOrigin, float radius)
    {
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= patrolInterval)
        {
            patrolTimer = 0f;
            Vector3 nextPos = SampleRandomNavPoint(centerOrigin, radius);
            navMeshAgent.SetDestination(nextPos);
        }
    }

    bool HasReachedDestination()
    {
        if (navMeshAgent.pathPending) return false;

        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            return navMeshAgent.velocity.sqrMagnitude < 0.05f;
        }

        return navMeshAgent.remainingDistance <= (navMeshAgent.stoppingDistance + 1.5f);
    }

    static Vector3 SampleRandomNavPoint(Vector3 origin, float radius)
    {
        Vector3 randomDir = Random.insideUnitSphere * radius + origin;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, radius, NavMesh.AllAreas))
            return hit.position;

        return origin;
    }
}