using UnityEngine;
using UnityEngine.AI;

public class AggroEntityWondering : MonoBehaviour
{
    public enum WanderState { Normal, Investigating, LocalSearch, Relocating }
    [Header("Current State (Debug)")]
    public WanderState currentState = WanderState.Normal;

    private NavMeshAgent navMeshAgent;

    [Header("Normal Wandering Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;

    [Header("After Losing Player Settings")]
    [SerializeField] private float localSearchDuration = 10f;
    [SerializeField] private float relocateDistance = 30f;

    private float timer;
    private float searchTimer;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnDisable()
    {
        currentState = WanderState.Normal;
    }

    private void Update()
    {
        switch (currentState)
        {
            case WanderState.Normal:
                PerformWandering(wanderRadius);
                break;

            case WanderState.Investigating:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentState = WanderState.LocalSearch;
                    searchTimer = 0f;
                    timer = wanderTimer;
                }
                break;

            case WanderState.LocalSearch:
                searchTimer += Time.deltaTime;

                if (searchTimer >= localSearchDuration)
                {
                    RelocateFarAway();
                }
                else
                {
                    PerformWandering(wanderRadius);
                }
                break;

            case WanderState.Relocating:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentState = WanderState.Normal;
                }
                break;
        }
    }

    private void PerformWandering(float radius)
    {
        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, radius, -1);
            navMeshAgent.SetDestination(newPos);
            timer = 0;
        }
    }

    public void InvestigateLocation(Vector3 targetLocation)
    {
        currentState = WanderState.Investigating;
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(targetLocation);
        }
    }

    private void RelocateFarAway()
    {
        currentState = WanderState.Relocating;
        Vector3 randomDirection = Random.insideUnitSphere.normalized * relocateDistance;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, relocateDistance / 2f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(navHit.position);
        }
        else
        {
            currentState = WanderState.Normal;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, dist, layermask))
        {
            return navHit.position;
        }
        return origin;
    }
}