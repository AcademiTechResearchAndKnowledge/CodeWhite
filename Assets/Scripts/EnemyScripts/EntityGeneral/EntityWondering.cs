using UnityEngine;
using UnityEngine.AI;

public class EntityWondering : MonoBehaviour
{
    // --- NEW: A State Machine to track what the AI is currently doing ---
    public enum WanderState { Normal, Investigating, LocalSearch, Relocating }
    [Header("Current State (Debug)")]
    public WanderState currentState = WanderState.Normal;

    private NavMeshAgent navMeshAgent;

    [Header("Normal Wandering Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;

    [Header("After Losing Player Settings")]
    [SerializeField] private float localSearchDuration = 10f; // How long to search the area where player hid
    [SerializeField] private float relocateDistance = 30f;    // How far away it should go to give the player a chance

    private float timer;
    private float searchTimer;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    // Reset the state if this script gets disabled (e.g., it spots the player again)
    private void OnDisable()
    {
        currentState = WanderState.Normal;
    }

    private void Update()
    {
        // Handle behavior based on current state
        switch (currentState)
        {
            case WanderState.Normal:
                PerformWandering(wanderRadius);
                break;

            case WanderState.Investigating:
                // Check if the agent has reached the last known location
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentState = WanderState.LocalSearch; // Switch to searching the area
                    searchTimer = 0f;
                    timer = wanderTimer; // Force an immediate wander step
                }
                break;

            case WanderState.LocalSearch:
                searchTimer += Time.deltaTime;

                // If we've searched long enough, it's time to leave
                if (searchTimer >= localSearchDuration)
                {
                    RelocateFarAway();
                }
                else
                {
                    // Continue wandering locally
                    PerformWandering(wanderRadius);
                }
                break;

            case WanderState.Relocating:
                // Wait until we reach the far away point
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentState = WanderState.Normal; // We arrived, back to normal roaming
                }
                break;
        }
    }

    // Extracted the normal wandering logic into a clean method
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

    // Called by EntityDetector when the player hides
    public void InvestigateLocation(Vector3 targetLocation)
    {
        currentState = WanderState.Investigating;
        if (navMeshAgent != null)
        {
            navMeshAgent.SetDestination(targetLocation);
        }
    }

    // --- NEW: Logic to find a point far away ---
    private void RelocateFarAway()
    {
        currentState = WanderState.Relocating;

        // Force a random direction but push it outward by the relocateDistance
        Vector3 randomDirection = Random.insideUnitSphere.normalized * relocateDistance;
        randomDirection += transform.position;

        // Sample the NavMesh to ensure it's a valid walkable point. 
        // We use a large sample radius (relocateDistance / 2f) just in case the exact math point lands inside a wall
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, relocateDistance / 2f, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(navHit.position);
        }
        else
        {
            // Failsafe: If the map is too small to find a point that far away, just go back to normal
            currentState = WanderState.Normal;
        }
    }

    // Generate a random point on the NavMesh within radius
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