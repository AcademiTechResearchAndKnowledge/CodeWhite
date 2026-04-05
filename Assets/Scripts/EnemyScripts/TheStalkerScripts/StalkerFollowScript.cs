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

    // This saves performance so we don't search for the tag every single frame
    private Transform cachedClosetTarget;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Called by the spawner for an IDLE spawn
    public void InitializeForIdle(PlayerReferences refs, float threshold)
    {
        playerRefs = refs;
        movementThreshold = threshold;
        spawnReason = SpawnReason.Idle;

        UpdateDestination();
    }

    // Called by the spawner for a CLOSET spawn
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
        // 1. If they are in the closet, hunt the "PlayerFollow" tag
        if (closetSystem != null && closetSystem.InsideCloset)
        {
            // Find the tag only if we haven't found it yet to save processing power
            if (cachedClosetTarget == null)
            {
                GameObject targetObj = GameObject.FindGameObjectWithTag("PlayerFollow");
                if (targetObj != null)
                {
                    cachedClosetTarget = targetObj.transform;
                }
            }

            // Move towards the closet camera/spot
            if (cachedClosetTarget != null)
            {
                agent.SetDestination(cachedClosetTarget.position);
            }
        }
        // 2. If they are NOT in the closet, hunt the actual player's transform directly
        else if (playerRefs != null)
        {
            agent.SetDestination(playerRefs.transform.position);
        }
    }

    private void CheckDespawnConditions()
    {
        if (spawnReason == SpawnReason.Idle)
        {
            // Despawn if they were idling and started moving
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
            // Despawn if they were in the closet and stepped out
            if (closetSystem != null && !closetSystem.InsideCloset)
            {
                Despawn();
            }
        }
    }

    private void Despawn()
    {
        Debug.Log("Stalker despawning: Player moved or left the closet.");
        Destroy(gameObject);
    }
}