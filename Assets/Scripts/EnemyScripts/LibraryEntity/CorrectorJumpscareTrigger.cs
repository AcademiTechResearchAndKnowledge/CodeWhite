using UnityEngine;

public class CorrectorJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    [Header("Anxiety Penalty")]
    [Tooltip("Percentage of Max Anxiety to add when caught (e.g., 25 means 25% of the bar).")]
    [Range(0f, 100f)]
    public float anxietySpikePercentage = 25f;

    private Transform playerTransform;
    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler; // NEW: Reference to pause the decay
    private bool hasCaughtPlayer = false;

    private AggroEntityDetector entityDetector;

    private void Start()
    {
        entityDetector = GetComponent<AggroEntityDetector>();

        if (entityDetector == null)
        {
            Debug.LogError("CorrectorJumpscareTrigger: Cannot find EntityDetector script on this entity!");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;

            playerStats = playerObj.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("CorrectorJumpscareTrigger: No PlayerStats found on the Player object!");
            }

            // NEW: Find the AnxietyHandler in the scene so we can reset its decay timer
            anxietyHandler = FindFirstObjectByType<AnxietyHandler>();
            if (anxietyHandler == null)
            {
                Debug.LogWarning("CorrectorJumpscareTrigger: No AnxietyHandler found in the scene.");
            }
        }
        else
        {
            Debug.LogError("CorrectorJumpscareTrigger: No object with tag 'Player' found in the scene.");
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

        // 1. Calculate and apply the Anxiety Spike
        if (playerStats != null)
        {
            float anxietyToAdd = (anxietySpikePercentage / 100f) * playerStats.MaxAnxiety;
            playerStats.AddStat(StatType.ANX, anxietyToAdd);

            Debug.Log($"[Anxiety System] Player caught! Added {anxietySpikePercentage}% ({anxietyToAdd} raw points) to Anxiety.");

            // THE FIX: Tell the AnxietyHandler to reset its cooldown timer.
            // This holds the anxiety at the spiked value for 'decayDelay' seconds before decaying.
            if (anxietyHandler != null)
            {
                anxietyHandler.ResetSafeTimer();
                Debug.Log("[Anxiety System] Safe timer reset. Decay paused.");
            }
        }

        // 2. Make the entity disappear
        Debug.Log("[Entity Action] Corrector jumpscare triggered. Entity is now disappearing.");
        Destroy(gameObject);
    }
}