using UnityEngine;

public class CorrectorJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    private Transform playerTransform;
    private bool hasCaughtPlayer = false;

    // We need a reference to your existing detector script
    private EntityDetector entityDetector;

    private void Start()
    {
        // Grab the EntityDetector component attached to this same GameObject
        entityDetector = GetComponent<EntityDetector>();

        if (entityDetector == null)
        {
            Debug.LogError("CorrectorJumpscareTrigger: Cannot find EntityDetector script on this entity!");
        }

        // Automatically find the player when the entity spawns
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("CorrectorJumpscareTrigger: No object with tag 'Player' found in the scene.");
        }
    }

    private void Update()
    {
        // Stop checking if we lost the player reference, already caught them, or missing the detector
        if (playerTransform == null || hasCaughtPlayer || entityDetector == null) return;

        // Check the current distance between this entity and the player
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // THE FIX: Only catch the player if they are within radius AND the entity is actively looking/chasing
        if (distanceToPlayer <= catchRadius && entityDetector.isLookingPlayer)
        {
            TriggerJumpscare();
        }
    }

    private void TriggerJumpscare()
    {
        // Lock this so it doesn't trigger multiple times in the split second before it destroys itself
        hasCaughtPlayer = true;

        // 1. Placeholder for the Anxiety System
        Debug.Log("[Anxiety System] Player caught by the Corrector! Anxiety increased by 25.");

        // 2. Make the entity disappear
        Debug.Log("[Entity Action] Corrector jumpscare triggered. Entity is now disappearing.");
        Destroy(gameObject);
    }
}