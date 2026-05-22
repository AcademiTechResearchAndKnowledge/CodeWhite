using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class DespawningEntityWondering : MonoBehaviour
{
    public enum WanderState { Normal, Investigating, LocalSearch }
    [Header("Current State (Debug)")]
    public WanderState currentState = WanderState.Normal;

    private NavMeshAgent navMeshAgent;

    [Header("Normal Wandering Settings")]
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderTimer = 5f;

    [Header("Investigation Settings")]
    [Tooltip("How far from the exact target location the entity will stand when investigating.")]
    [SerializeField] private float investigationOffset = 3f;

    [Header("NavMesh Edge Buffer")]
    [Tooltip("How far away from the NavMesh edge the entity must stay.")]
    [SerializeField] private float edgeBufferDistance = 2f;
    [Tooltip("How many times to try finding a valid point before giving up for this frame.")]
    [SerializeField] private int maxPointRetries = 10;

    [Header("Anti-Stagnation Settings")]
    [Tooltip("If the entity stays within this radius...")]
    [SerializeField] private float stagnationRadius = 15f;
    [Tooltip("...for this many seconds, it will force a despawn to avoid being stuck.")]
    [SerializeField] private float maxStagnationTime = 30f;

    [Header("Breadcrumb Path Avoidance")]
    [Tooltip("How many recent locations the entity remembers.")]
    [SerializeField] private int maxBreadcrumbs = 3;
    [Tooltip("The entity will reject new random points within this radius of a remembered location.")]
    [SerializeField] private float breadcrumbAvoidanceRadius = 6f;

    [Header("After Losing Player Settings")]
    [SerializeField] private float localSearchDuration = 10f;

    // --- ADDED: How long they wander after losing you by distance before despawning ---
    [SerializeField] private float normalWanderDuration = 15f;

    private float timer;
    private float searchTimer;

    // --- ADDED: Tracker for the normal wander timer ---
    private float normalWanderTimer;

    // Stagnation tracking variables
    private Vector3 stagnationCenter;
    private float currentStagnationTime;

    // Breadcrumb memory
    private List<Vector3> breadcrumbs = new List<Vector3>();

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        stagnationCenter = transform.position;
        currentStagnationTime = 0f;
        normalWanderTimer = 0f; // Reset timer
        breadcrumbs.Clear();
    }

    private void OnDisable()
    {
        currentState = WanderState.Normal;
    }

    public void StartWanderingInstantly()
    {
        currentState = WanderState.Normal;
        timer = wanderTimer;
        stagnationCenter = transform.position;
        currentStagnationTime = 0f;
        normalWanderTimer = 0f; // Reset timer
        breadcrumbs.Clear();
    }

    private void Update()
    {
        switch (currentState)
        {
            case WanderState.Normal:
                // --- ADDED: Despawn if they wander around for too long without seeing you ---
                normalWanderTimer += Time.deltaTime;
                if (normalWanderTimer >= normalWanderDuration)
                {
                    DespawnEntity();
                }
                else
                {
                    CheckStagnation();
                    PerformWandering(wanderRadius, wanderTimer);
                }
                break;

            case WanderState.Investigating:
                if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
                {
                    currentState = WanderState.LocalSearch;
                    searchTimer = 0f;
                    timer = 2f;
                }
                break;

            case WanderState.LocalSearch:
                searchTimer += Time.deltaTime;

                if (searchTimer >= localSearchDuration)
                {
                    DespawnEntity();
                }
                else
                {
                    PerformWandering(wanderRadius, 2f);
                }
                break;
        }
    }

    private void PerformWandering(float radius, float customTimer)
    {
        if (!navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            timer += Time.deltaTime;

            if (timer >= customTimer)
            {
                Vector3 newPos = GetValidWanderPosition(transform.position, radius, NavMesh.AllAreas);

                if (newPos != transform.position)
                {
                    navMeshAgent.SetDestination(newPos);
                    AddBreadcrumb(newPos);
                    timer = 0;
                }
                else
                {
                    timer = customTimer - 0.2f;
                }
            }
        }
    }

    private void CheckStagnation()
    {
        if (Vector3.Distance(transform.position, stagnationCenter) > stagnationRadius)
        {
            stagnationCenter = transform.position;
            currentStagnationTime = 0f;
        }
        else
        {
            currentStagnationTime += Time.deltaTime;

            if (currentStagnationTime >= maxStagnationTime)
            {
                DespawnEntity();
            }
        }
    }

    public void InvestigateLocation(Vector3 targetLocation)
    {
        currentState = WanderState.Investigating;
        if (navMeshAgent != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized * investigationOffset;
            Vector3 offsetPosition = targetLocation + new Vector3(randomDir.x, 0, randomDir.y);

            if (NavMesh.SamplePosition(offsetPosition, out NavMeshHit hit, investigationOffset * 2f, NavMesh.AllAreas))
            {
                navMeshAgent.SetDestination(hit.position);
            }
            else
            {
                navMeshAgent.SetDestination(targetLocation);
            }
        }
    }

    private void AddBreadcrumb(Vector3 position)
    {
        breadcrumbs.Add(position);

        if (breadcrumbs.Count > maxBreadcrumbs)
        {
            breadcrumbs.RemoveAt(0);
        }
    }

    private Vector3 GetValidWanderPosition(Vector3 origin, float dist, int layermask)
    {
        for (int i = 0; i < maxPointRetries; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * dist;
            randomDirection += origin;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, dist, layermask))
            {
                if (NavMesh.FindClosestEdge(navHit.position, out NavMeshHit edgeHit, layermask))
                {
                    if (edgeHit.distance < edgeBufferDistance) continue;
                }

                bool isTooCloseToBreadcrumb = false;
                foreach (Vector3 crumb in breadcrumbs)
                {
                    if (Vector3.Distance(navHit.position, crumb) < breadcrumbAvoidanceRadius)
                    {
                        isTooCloseToBreadcrumb = true;
                        break;
                    }
                }

                if (isTooCloseToBreadcrumb) continue;

                return navHit.position;
            }
        }
        return origin;
    }

    private void DespawnEntity()
    {
        Debug.Log("Search timer ended or entity stagnated. Despawning entity...");

        EntityDespawner despawner = GetComponent<EntityDespawner>();
        if (despawner != null)
        {
            despawner.DespawnWithParticles();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (breadcrumbs == null || breadcrumbs.Count == 0) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        foreach (Vector3 crumb in breadcrumbs)
        {
            Gizmos.DrawSphere(crumb, breadcrumbAvoidanceRadius);
        }
    }
}