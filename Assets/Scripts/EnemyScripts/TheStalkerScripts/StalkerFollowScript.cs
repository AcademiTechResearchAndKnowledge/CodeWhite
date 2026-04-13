using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StalkerFollowScript : MonoBehaviour
{
    private NavMeshAgent agent;
    private PlayerReferences playerRefs;
    private ClosetHidingSystem closetSystem;
    private float movementThreshold;

    private enum SpawnReason { Idle, Closet }
    private SpawnReason spawnReason;

    private Transform cachedClosetTarget;

    [Tooltip("The particle effect to spawn when the entity disappears.")]
    [SerializeField] private GameObject despawnParticlePrefab;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void InitializeForIdle(PlayerReferences refs, float threshold)
    {
        playerRefs = refs;
        movementThreshold = threshold;
        spawnReason = SpawnReason.Idle;

        UpdateDestination();
    }

    public void InitializeForCloset(PlayerReferences refs, ClosetHidingSystem closet)
    {
        playerRefs = refs;
        closetSystem = closet;
        spawnReason = SpawnReason.Closet;

        UpdateDestination();
    }

    private void Update()
    {
        UpdateDestination();
        CheckDespawnConditions();
    }

    private void UpdateDestination()
    {
        if (closetSystem != null && closetSystem.InsideCloset)
        {
            if (cachedClosetTarget == null)
            {
                GameObject targetObj = GameObject.FindGameObjectWithTag("PlayerFollow");
                if (targetObj != null)
                {
                    cachedClosetTarget = targetObj.transform;
                }
            }

            if (cachedClosetTarget != null)
            {
                agent.SetDestination(cachedClosetTarget.position);
            }
        }
        else if (playerRefs != null)
        {
            agent.SetDestination(playerRefs.transform.position);
        }
    }

    private void CheckDespawnConditions()
    {
        if (spawnReason == SpawnReason.Idle)
        {
            if (playerRefs != null && playerRefs.rb != null)
            {
                Vector3 flatVelocity = new Vector3(playerRefs.rb.linearVelocity.x, 0f, playerRefs.rb.linearVelocity.z);
                if (flatVelocity.magnitude > movementThreshold)
                {
                    Despawn();
                }
            }
        }
        else if (spawnReason == SpawnReason.Closet)
        {
            if (closetSystem != null && !closetSystem.InsideCloset)
            {
                Despawn();
            }
        }
    }

    private void Despawn()
    {
        Debug.Log("Stalker despawning: Player moved or left the closet.");

        if (despawnParticlePrefab != null)
        {
            Instantiate(despawnParticlePrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("Despawn Particle Prefab is not assigned on " + gameObject.name);
        }

        Destroy(gameObject);
    }
}