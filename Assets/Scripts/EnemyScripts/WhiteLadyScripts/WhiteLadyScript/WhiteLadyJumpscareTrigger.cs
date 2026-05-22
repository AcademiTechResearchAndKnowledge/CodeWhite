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
    [SerializeField] private JumpscareMechanic canvasJumpscare;

    private Transform playerTransform;
    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;
    private bool hasCaughtPlayer = false;

    private WhiteLady whiteLady;

    private void Start()
    {
        whiteLady = GetComponent<WhiteLady>();

        if (canvasJumpscare == null)
            canvasJumpscare = FindAnyObjectByType<JumpscareMechanic>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (playerObj != null) playerTransform = playerObj.transform;

        GameObject actualPlayer = GameObject.FindGameObjectWithTag("Player");
        if (actualPlayer != null)
        {
            playerStats = actualPlayer.GetComponent<PlayerStats>();
            anxietyHandler = actualPlayer.GetComponent<AnxietyHandler>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || hasCaughtPlayer || whiteLady == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Note: We do NOT need the spatial anxiety aura logic here, 
        // because your WhiteLady.cs already has UpdateAnxietyAura() built into her state machine!

        // Check if she is close enough AND actively chasing you
        if (distanceToPlayer <= catchRadius && whiteLady.CurrentState == WhiteLady.State.Chasing)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    private IEnumerator JumpscareRoutine()
    {
        hasCaughtPlayer = true;

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

        Vector3 lookPos = playerTransform.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookPos);

        // 4. Trigger UI Canvas
        float waitTime = 2.0f;
        if (canvasJumpscare != null)
        {
            canvasJumpscare.TriggerJumpscare();
            waitTime = canvasJumpscare.animationDuration - 0.5f;
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