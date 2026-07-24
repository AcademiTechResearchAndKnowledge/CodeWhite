using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(WhiteLady))]
public class WhiteLadyJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    [Header("Anxiety Jumpscare Penalty")]
    [Tooltip("Percentage of Max Anxiety to add when caught.")]
    [Range(0f, 100f)]
    public float anxietySpikePercentage = 25f;

    [Header("Canvas Jumpscare Setup")]
    [Tooltip("Drag the specific Jumpscare Canvas PREFAB for this entity here.")]
    [SerializeField] private JumpscareMechanic jumpscarePrefab;

    private bool hasCaughtPlayer = false;
    private WhiteLady whiteLady;

    private void Start()
    {
        whiteLady = GetComponent<WhiteLady>();
        // We removed the GameObject.Find calls. 
        // We will safely rely on the master WhiteLady script's playerRef instead!
    }

    private void Update()
    {
        // Bail if she is missing, already caught the player, or hasn't found the player yet
        if (hasCaughtPlayer || whiteLady == null || whiteLady.playerRef == null) return;

        // 1. Get the EXACT target she is chasing from the master script
        Transform targetTransform = whiteLady.playerRef.transform;

        // 2. Calculate Flat Distance (Ignore Y-axis height differences)
        Vector3 enemyPos = transform.position;
        Vector3 playerPos = targetTransform.position;

        enemyPos.y = 0f;
        playerPos.y = 0f;

        float distanceToPlayer = Vector3.Distance(enemyPos, playerPos);

        // 3. Check if she is close enough AND actively chasing
        if (distanceToPlayer <= catchRadius && whiteLady.CurrentState == WhiteLady.State.Chasing)
        {
            StartCoroutine(JumpscareRoutine(targetTransform.gameObject));
        }
    }

    private IEnumerator JumpscareRoutine(GameObject playerObj)
    {
        hasCaughtPlayer = true;

        // Grab stats dynamically so we don't have to rely on string tags
        PlayerStats playerStats = playerObj.GetComponent<PlayerStats>();
        AnxietyHandler anxietyHandler = playerObj.GetComponent<AnxietyHandler>();

        // 1. Shut down her master state machine and supporting scripts
        if (whiteLady != null) whiteLady.enabled = false;

        WhiteLadyWander wander = GetComponent<WhiteLadyWander>();
        if (wander != null) wander.enabled = false;

        WhiteLadyDetection detection = GetComponent<WhiteLadyDetection>();
        if (detection != null) detection.enabled = false;

        // 2. Stop movement
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 3. Stop animations and face the player
        SpriteDirectionalController dirController = GetComponentInChildren<SpriteDirectionalController>();
        if (dirController != null) dirController.enabled = false;

        Vector3 lookPos = playerObj.transform.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookPos);

        // 4. Trigger UI Canvas using the Prefab approach
        float waitTime = 2.0f;

        if (jumpscarePrefab != null)
        {
            JumpscareMechanic spawnedJumpscare = Instantiate(jumpscarePrefab);
            spawnedJumpscare.TriggerJumpscare();
            waitTime = spawnedJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(waitTime);

        // 5. Hide sprite, apply anxiety penalty, and destroy
        SpriteRenderer entitySprite = GetComponentInChildren<SpriteRenderer>();
        if (entitySprite != null) entitySprite.enabled = false;

        if (playerStats != null)
        {
            float anxietyToAdd = (anxietySpikePercentage / 100f) * playerStats.MaxAnxiety;
            playerStats.AddStat(StatType.ANX, anxietyToAdd);
            if (anxietyHandler != null) anxietyHandler.ResetSafeTimer();
        }

        Destroy(gameObject);
    }
}