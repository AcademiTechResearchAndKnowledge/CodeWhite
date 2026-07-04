using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RelentlessJumpscare : MonoBehaviour
{
    public float catchRadius = 2.5f;

    [Range(0f, 100f)]
    public float anxietySpikePercentage = 25f;

    public float anxietyAuraRadius = 15f;

    public AnimationCurve anxietyDistanceCurve = new AnimationCurve(
        new Keyframe(0f, 50f),
        new Keyframe(5f, 25f),
        new Keyframe(10f, 10f),
        new Keyframe(15f, 0f)
    );

    [SerializeField] private JumpscareMechanic jumpscarePrefab;

    private Transform playerTransform;
    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;
    private bool hasCaughtPlayer = false;
    private RelentlessChaserAI chaserAI;

    private void Start()
    {
        chaserAI = GetComponent<RelentlessChaserAI>();

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
            playerStats = playerTransform.GetComponentInParent<PlayerStats>();
            anxietyHandler = playerTransform.GetComponentInParent<AnxietyHandler>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || hasCaughtPlayer) return;

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

        if (distanceToPlayer <= catchRadius)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    private IEnumerator JumpscareRoutine()
    {
        hasCaughtPlayer = true;

        if (chaserAI != null) chaserAI.enabled = false;

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

        float waitTime = 2f;

        if (jumpscarePrefab != null)
        {
            JumpscareMechanic spawnedJumpscare = Instantiate(jumpscarePrefab);
            spawnedJumpscare.TriggerJumpscare();
            waitTime = spawnedJumpscare.animationDuration - 0.5f;
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

        DeathMenu deathMenu = FindFirstObjectByType<DeathMenu>();
        if (deathMenu != null)
        {
            deathMenu.TriggerDeath();
        }

        yield return null;

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