using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CandleJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    [Header("Anxiety Penalty")]
    [Tooltip("Percentage of Max Anxiety to add when caught (e.g., 25 means 25% of the bar).")]
    [Range(0f, 100f)]
    public float anxietySpikePercentage = 25f;

    [Header("Anxiety Proximity (Spatial)")]
    [Tooltip("The maximum distance the aura reaches.")]
    public float anxietyAuraRadius = 15f;
    [Tooltip("Graph: X-Axis is Distance, Y-Axis is Total Anxiety applied. Set X=0, Y=50 (close) and X=10, Y=10 (far).")]
    public AnimationCurve anxietyDistanceCurve = new AnimationCurve(
        new Keyframe(0f, 60f),   // Right on top of the player: 50 Anxiety
        new Keyframe(5f, 50f),   // 5 units away: 25 Anxiety
        new Keyframe(10f, 20f),  // 10 units away: 10 Anxiety
        new Keyframe(15f, 10f)    // Edge of radius: 0 Anxiety
    );

    [Header("Canvas Jumpscare Setup")]
    [Tooltip("Drag the specific Jumpscare Canvas PREFAB for this entity here.")]
    [SerializeField] private JumpscareMechanic jumpscarePrefab;

    private Transform playerTransform;
    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;
    private bool hasCaughtPlayer = false;

    private AggroEntityDetector entityDetector;

    // Tracks the highest anxiety this specific entity has already applied
    private float highestAnxietyApplied = 0f;

    private void Start()
    {
        entityDetector = GetComponent<AggroEntityDetector>();

        if (entityDetector == null)
        {
            Debug.LogError("CandleJumpscareTrigger: Cannot find AggroEntityDetector script on this entity!");
        }

        // REMOVED: FindAnyObjectByType so it doesn't grab the wrong canvas in the scene!

        // 1. Find PlayerFollow for the distance check
        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("CandleJumpscareTrigger: No object with tag 'PlayerFollow' found in the scene.");
        }

        // 2. Find the actual Player for the stats (Anxiety)
        GameObject actualPlayer = GameObject.FindGameObjectWithTag("Player");
        if (actualPlayer != null)
        {
            playerStats = actualPlayer.GetComponent<PlayerStats>();
            if (playerStats == null)
            {
                Debug.LogError("CandleJumpscareTrigger: No PlayerStats found on the Player object!");
            }

            // Grab the AnxietyHandler directly from the Player object
            anxietyHandler = actualPlayer.GetComponent<AnxietyHandler>();
            if (anxietyHandler == null)
            {
                Debug.LogWarning("CandleJumpscareTrigger: No AnxietyHandler found on the Player object!");
            }
        }
        else
        {
            Debug.LogError("CandleJumpscareTrigger: No object with tag 'Player' found in the scene for stats.");
        }
    }

    private void Update()
    {
        if (playerTransform == null || hasCaughtPlayer || entityDetector == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // --- NEW: Spatial Distance Anxiety ---
        if (distanceToPlayer <= anxietyAuraRadius && playerStats != null)
        {
            // Check the curve to see how much total anxiety the player SHOULD have taken by now
            float targetAnxiety = anxietyDistanceCurve.Evaluate(distanceToPlayer);

            // If the target is higher than what we have already given them, apply the difference
            if (targetAnxiety > highestAnxietyApplied)
            {
                float amountToAdd = targetAnxiety - highestAnxietyApplied;
                playerStats.AddStat(StatType.ANX, amountToAdd);

                highestAnxietyApplied = targetAnxiety; // Remember that we applied this much

                if (anxietyHandler != null)
                {
                    anxietyHandler.ResetSafeTimer();
                }
            }
        }
        // -------------------------------------

        if (distanceToPlayer <= catchRadius && entityDetector.isLookingPlayer)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    private IEnumerator JumpscareRoutine()
    {
        hasCaughtPlayer = true;

        // 1. Freeze the entity
        if (entityDetector != null) entityDetector.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        SpriteDirectionalController dirController = GetComponentInChildren<SpriteDirectionalController>();
        if (dirController != null) dirController.enabled = false;

        // 2. Face the player
        Vector3 lookPos = playerTransform.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookPos);
        }

        // 3. Trigger Canvas Jumpscare using the Prefab approach
        float waitTime = 2.0f;
        if (jumpscarePrefab != null)
        {
            JumpscareMechanic spawnedJumpscare = Instantiate(jumpscarePrefab);
            spawnedJumpscare.TriggerJumpscare();
            waitTime = spawnedJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(waitTime);

        // Hide the sprite right before dealing stats and despawning
        SpriteRenderer entitySprite = GetComponentInChildren<SpriteRenderer>();
        if (entitySprite != null) entitySprite.enabled = false;

        // 4. Calculate and apply the Anxiety Spike
        if (playerStats != null)
        {
            float anxietyToAdd = (anxietySpikePercentage / 100f) * playerStats.MaxAnxiety;
            playerStats.AddStat(StatType.ANX, anxietyToAdd);
            Debug.Log($"[Anxiety System] Player caught by Candle Entity! Added {anxietySpikePercentage}% ({anxietyToAdd} raw points) to Anxiety.");

            if (anxietyHandler != null)
            {
                anxietyHandler.ResetSafeTimer();
                Debug.Log("[Anxiety System] Safe timer reset via Candle Jumpscare. Decay paused.");
            }
        }

        // 5. PENALTY: Tell the manager to blow out a candle and hide the lighter
        if (LighterPuzzleManager.instance != null)
        {
            Debug.Log("[Puzzle System] Blowing out the candle due to Jumpscare.");
            LighterPuzzleManager.instance.BlowOutCandle();
        }

        // 6. Entity Disappears 
        Debug.Log("[Entity Action] Candle Entity is now despawning.");
        Destroy(gameObject);
    }
}