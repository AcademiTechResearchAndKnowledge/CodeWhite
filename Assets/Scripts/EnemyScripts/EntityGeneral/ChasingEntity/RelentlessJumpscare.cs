using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RelentlessJumpscare : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    [Header("Anxiety Jumpscare Penalty")]
    [Tooltip("Percentage of Max Anxiety to add when caught.")]
    [Range(0f, 100f)]
    public float anxietySpikePercentage = 25f;

    [Header("Anxiety Proximity (Spatial)")]
    [Tooltip("The maximum distance the aura reaches.")]
    public float anxietyAuraRadius = 15f;
    [Tooltip("Graph: X-Axis is Distance, Y-Axis is Total Anxiety applied.")]
    public AnimationCurve anxietyDistanceCurve = new AnimationCurve(
        new Keyframe(0f, 50f),
        new Keyframe(5f, 25f),
        new Keyframe(10f, 10f),
        new Keyframe(15f, 0f)
    );

    [Header("Canvas Jumpscare Setup")]
    [Tooltip("Drag the specific Jumpscare Canvas PREFAB for this entity here.")]
    [SerializeField] private JumpscareMechanic jumpscarePrefab;

    private Transform playerTransform;
    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;
    private bool hasCaughtPlayer = false;
    private RelentlessChaserAI chaserAI;

    private void Start()
    {
        chaserAI = GetComponent<RelentlessChaserAI>();

        // REMOVED: FindAnyObjectByType so it doesn't grab the wrong canvas!

        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (playerObj != null) playerTransform = playerObj.transform;

        GameObject actualPlayer = GameObject.FindGameObjectWithTag("Player");
        if (actualPlayer != null)
        {
            playerStats = actualPlayer.GetComponent<PlayerStats>();
            anxietyHandler = actualPlayer.GetComponent<AnxietyHandler>();
        }
        else if (playerTransform != null)
        {
            // Fallback if tags are structured differently
            playerStats = playerTransform.GetComponentInParent<PlayerStats>();
            anxietyHandler = playerTransform.GetComponentInParent<AnxietyHandler>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || hasCaughtPlayer) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // „Ÿ„Ÿ„Ÿ Spatial Distance Anxiety „Ÿ„Ÿ„Ÿ
        if (anxietyHandler != null)
        {
            if (distanceToPlayer <= anxietyAuraRadius)
            {
                float targetAnxiety = anxietyDistanceCurve.Evaluate(distanceToPlayer);
                anxietyHandler.externalProximityFloor = targetAnxiety;
            }
            else
            {
                anxietyHandler.externalProximityFloor = 0f;
            }
        }
        // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

        // Trigger jumpscare simply based on distance
        if (distanceToPlayer <= catchRadius)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    private IEnumerator JumpscareRoutine()
    {
        hasCaughtPlayer = true;

        // Disable AI movement
        if (chaserAI != null) chaserAI.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // Lock rotation onto player for the jumpscare
        SpriteDirectionalController dirController = GetComponentInChildren<SpriteDirectionalController>();
        if (dirController != null) dirController.enabled = false;

        Vector3 lookPos = playerTransform.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookPos);

        float waitTime = 2.0f;

        // --- NEW LOGIC: Spawn the specific jumpscare prefab! ---
        if (jumpscarePrefab != null)
        {
            JumpscareMechanic spawnedJumpscare = Instantiate(jumpscarePrefab);
            spawnedJumpscare.TriggerJumpscare();
            waitTime = spawnedJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(waitTime);

        // Hide sprite during cleanup
        SpriteRenderer entitySprite = GetComponentInChildren<SpriteRenderer>();
        if (entitySprite != null) entitySprite.enabled = false;

        // Apply anxiety penalty
        if (playerStats != null)
        {
            float anxietyToAdd = (anxietySpikePercentage / 100f) * playerStats.MaxAnxiety;
            playerStats.AddStat(StatType.ANX, anxietyToAdd);
            if (anxietyHandler != null) anxietyHandler.ResetSafeTimer();
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Clean up the anxiety floor when destroyed
        if (anxietyHandler != null)
        {
            anxietyHandler.externalProximityFloor = 0f;
        }
    }
}