using UnityEngine;

public class CorrectorJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    // „Ÿ„Ÿ„Ÿ NEW: Anxiety Spike Settings „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    [Header("Anxiety Penalty")]
    [Tooltip("Percentage of Max Anxiety to add when caught (e.g., 25 means 25% of the bar).")]
    [Range(0f, 100f)]
    public float anxietySpikePercentage = 25f;

    private Transform playerTransform;
    private PlayerStats playerStats; // Reference to apply the anxiety spike
    private bool hasCaughtPlayer = false;

    // We need a reference to your existing detector script
    private AggroEntityDetector entityDetector;

    private void Start()
    {
        // Grab the EntityDetector component attached to this same GameObject
        entityDetector = GetComponent<AggroEntityDetector>();

        if (entityDetector == null)
        {
            Debug.LogError("CorrectorJumpscareTrigger: Cannot find EntityDetector script on this entity!");
        }

        // Automatically find the player when the entity spawns
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;

            // NEW: Grab the PlayerStats component so we can modify anxiety later
            playerStats = playerObj.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("CorrectorJumpscareTrigger: No PlayerStats found on the Player object!");
            }
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

        // 1. Calculate and apply the Anxiety Spike
        if (playerStats != null)
        {
            // Calculate exact points to add based on the percentage of MaxAnxiety
            float anxietyToAdd = (anxietySpikePercentage / 100f) * playerStats.MaxAnxiety;

            // Add it to the player's stats
            playerStats.AddStat(StatType.ANX, anxietyToAdd);

            Debug.Log($"[Anxiety System] Player caught! Added {anxietySpikePercentage}% ({anxietyToAdd} raw points) to Anxiety.");
        }

        // 2. Make the entity disappear
        Debug.Log("[Entity Action] Corrector jumpscare triggered. Entity is now disappearing.");
        Destroy(gameObject);
    }
}