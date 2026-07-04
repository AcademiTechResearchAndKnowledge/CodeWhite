using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EntityAi : MonoBehaviour
{
    public int area_ID;
    public PlayerActionDetector detector;
    private Transform playerTarget;
    private NavMeshAgent agent;
    public DialogueTrigger DT;
    private Vector3 startPos;
    private Quaternion startRot;

    public Transform returnPoint;
    [SerializeField] private float catchDistance = 2f;
    private bool isActive = false;
    private bool isResetting = false;

    [Header("Jumpscare Settings")]
    [Tooltip("Drag the specific Jumpscare Canvas PREFAB for this entity here.")]
    [SerializeField] private JumpscareMechanic jumpscarePrefab;

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

    private Transform playerTransform;
    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;
    private bool hasCaughtPlayer = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        startPos = transform.position;
        startRot = transform.rotation;
        if (returnPoint == null)
            returnPoint = new GameObject("ReturnPoint").transform;
        returnPoint.position = startPos;
        returnPoint.rotation = startRot;
    }

    private void Start()
    {
        GameObject playerFollowObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (playerFollowObj != null) playerTransform = playerFollowObj.transform;

        GameObject actualPlayer = GameObject.FindGameObjectWithTag("Player");
        if (actualPlayer != null)
        {
            playerStats = actualPlayer.GetComponent<PlayerStats>();
            anxietyHandler = actualPlayer.GetComponent<AnxietyHandler>();
        }
    }

    [System.Obsolete]
    private void Update()
    {
        if (isResetting || hasCaughtPlayer) return;

        HandleAnxietyAura();

        if (isActive && playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist <= catchDistance)
            {
                StartCoroutine(JumpscareRoutine());
            }
        }
        else if (!isActive && agent != null && Vector3.Distance(transform.position, returnPoint.position) > 0.1f)
        {
            agent.SetDestination(returnPoint.position);
        }
    }

    private void HandleAnxietyAura()
    {
        if (anxietyHandler == null) return;

        Transform target = playerTarget != null ? playerTarget : playerTransform;
        if (target == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

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

    private IEnumerator JumpscareRoutine()
    {
        hasCaughtPlayer = true;
        isActive = false;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        SpriteDirectionalController dirController = GetComponentInChildren<SpriteDirectionalController>();
        if (dirController != null) dirController.enabled = false;

        Transform lookTarget = playerTarget != null ? playerTarget : playerTransform;
        if (lookTarget != null)
        {
            Vector3 lookPos = lookTarget.position - transform.position;
            lookPos.y = 0f;
            if (lookPos != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookPos);
        }

        float waitTime = 2.0f;

        if (jumpscarePrefab != null)
        {
            JumpscareMechanic spawnedJumpscare = Instantiate(jumpscarePrefab);
            spawnedJumpscare.TriggerJumpscare();
            waitTime = spawnedJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(waitTime);

        if (DT != null)
        {
            DT.TriggerDialogue();
        }

        if (playerStats != null)
        {
            float anxietyToAdd = (anxietySpikePercentage / 100f) * playerStats.MaxAnxiety;
            playerStats.AddStat(StatType.ANX, anxietyToAdd);
            if (anxietyHandler != null) anxietyHandler.ResetSafeTimer();
        }

        if (anxietyHandler != null)
        {
            anxietyHandler.externalProximityFloor = 0f;
        }

        if (dirController != null) dirController.enabled = true;

        detector.ResetPlayerAndNPCs();
    }

    public void Activate()
    {
        isActive = false;
        playerTarget = null;
    }

    public void StartChase(Transform player)
    {
        if (isResetting || hasCaughtPlayer) return;
        playerTarget = player;
        isActive = true;
    }

    public void ResetNPC()
    {
        isResetting = true;
        isActive = false;
        playerTarget = null;
        hasCaughtPlayer = false;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(returnPoint.position);
        }

        isResetting = false;
    }
}