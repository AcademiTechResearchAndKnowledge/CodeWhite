using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DespawningJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    public float catchRadius = 2.5f;

    [Header("Anxiety Jumpscare Penalty")]
    [Range(0f, 100f)]
    public float anxietySpikePercentage = 25f;

    [Header("Anxiety Proximity (Spatial)")]
    public float anxietyAuraRadius = 15f;
    public AnimationCurve anxietyDistanceCurve = new AnimationCurve(
        new Keyframe(0f, 50f),
        new Keyframe(5f, 25f),
        new Keyframe(10f, 10f),
        new Keyframe(15f, 0f)
    );

    [Header("Canvas Jumpscare Setup")]
    [SerializeField] private JumpscareMechanic canvasJumpscare;

    private Transform playerTransform;
    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;
    private bool hasCaughtPlayer = false;
    private DespawningEntityDetector entityDetector;

    private void Start()
    {
        entityDetector = GetComponent<DespawningEntityDetector>();

        if (entityDetector == null)
            Debug.LogError("DespawningJumpscareTrigger: Cannot find DespawningEntityDetector script!");

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
        if (playerTransform == null || hasCaughtPlayer || entityDetector == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

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

        if (distanceToPlayer <= catchRadius && entityDetector.isLookingPlayer)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    private IEnumerator JumpscareRoutine()
    {
        hasCaughtPlayer = true;
        if (entityDetector != null) entityDetector.enabled = false;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        SpriteDirectionalController dirController = GetComponentInChildren<SpriteDirectionalController>();
        if (dirController != null) dirController.enabled = false;

        Vector3 lookPos = playerTransform.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookPos);

        float waitTime = 2.0f;
        if (canvasJumpscare != null)
        {
            canvasJumpscare.TriggerJumpscare();
            waitTime = canvasJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(waitTime);

        SpriteRenderer entitySprite = GetComponentInChildren<SpriteRenderer>();
        if (entitySprite != null) entitySprite.enabled = false;

        if (playerStats != null)
        {
            float anxietyToAdd = (anxietySpikePercentage / 100f) * playerStats.MaxAnxiety;
            playerStats.AddStat(StatType.ANX, anxietyToAdd);
            if (anxietyHandler != null) anxietyHandler.ResetSafeTimer();
        }

        // --- MODIFIED: Removed the particle despawner. Now it just dies silently. ---
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (anxietyHandler != null)
        {
            anxietyHandler.externalProximityFloor = 0f;
        }
    }
}