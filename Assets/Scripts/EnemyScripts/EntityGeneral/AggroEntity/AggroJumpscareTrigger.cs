using UnityEngine;

[RequireComponent(typeof(EntityDespawner))] // This ensures the new script is always attached!
public class AggroJumpscareTrigger : MonoBehaviour
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

        // 1. Find PlayerFollow for the distance check
        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("AggroJumpscareTrigger: No object with tag 'PlayerFollow' found in the scene.");
        }

        // 2. Find the actual Player for the stats (Anxiety)
        GameObject actualPlayer = GameObject.FindGameObjectWithTag("Player");
        if (actualPlayer != null)
        {
            playerStats = actualPlayer.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("AggroJumpscareTrigger: No PlayerStats found on the Player object!");
            }
        }
        else
        {
            Debug.LogError("AggroJumpscareTrigger: No object with tag 'Player' found in the scene for stats.");
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
        }

        // 2. Jumpscare Effects
        Debug.Log("[Jumpscare System] Jumpscare sequence initiated.");

        // 3. Entity Disappears using the despawner script
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