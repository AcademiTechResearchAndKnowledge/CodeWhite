using UnityEngine;

[RequireComponent(typeof(EntityDespawner))] // This ensures the new script is always attached!
public class AggroJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    private Transform playerTransform;
    private bool hasCaughtPlayer = false;

    private AggroEntityDetector entityDetector;
    private EntityDespawner despawner; // Reference to our new script

    private void Start()
    {
        entityDetector = GetComponent<AggroEntityDetector>();
        despawner = GetComponent<EntityDespawner>(); // Grab the script

        if (entityDetector == null)
        {
            Debug.LogError("AggroJumpscareTrigger: Cannot find AggroEntityDetector script on this entity!");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("AggroJumpscareTrigger: No object with tag 'PlayerFollow' found in the scene.");
        }
    }

    private void Update()
    {
        if (playerTransform == null || hasCaughtPlayer || entityDetector == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= catchRadius && entityDetector.isLookingPlayer)
        {
            TriggerJumpscare();
        }
    }

    private void TriggerJumpscare()
    {
        hasCaughtPlayer = true;

        // 1. Placeholder for the Anxiety System
        Debug.Log("[Anxiety System] Player caught! Anxiety increased by 25.");

        // 2. Jumpscare Effects
        Debug.Log("[Jumpscare System] Jumpscare sequence initiated.");

        // 3. Entity Disappears using the new script!
        Debug.Log("[Entity Action] Entity is now disappearing.");

        if (despawner != null)
        {
            despawner.DespawnWithParticles();
        }
        else
        {
            // Fallback just in case the component is missing
            Destroy(gameObject);
        }
    }
}