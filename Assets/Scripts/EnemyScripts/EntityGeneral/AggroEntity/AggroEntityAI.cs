using UnityEngine;
using UnityEngine.AI;

public class AggroEntityAI : MonoBehaviour
{
    public Transform movePositionTransform;
    private NavMeshAgent navMeshAgent;

    [Header("Optimization Settings")]
    [Tooltip("How often (in seconds) the entity recalculates its path to the target.")]
    public float repathInterval = 0.2f;

    [Tooltip("Only recalculate instantly if the target moves more than this distance.")]
    public float repathDistanceThreshold = 1.0f;

    private float repathTimer;
    private Vector3 lastTargetPosition;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (movePositionTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerFollow");
            if (playerObj != null)
            {
                movePositionTransform = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        if (movePositionTransform != null && navMeshAgent.enabled)
        {
            repathTimer += Time.deltaTime;

            // Check if it's time to update OR if the target has moved significantly
            bool timeToUpdate = repathTimer >= repathInterval;
            bool targetMoved = (movePositionTransform.position - lastTargetPosition).sqrMagnitude > (repathDistanceThreshold * repathDistanceThreshold);
            if (timeToUpdate || targetMoved)
            {
                navMeshAgent.SetDestination(movePositionTransform.position);

                lastTargetPosition = movePositionTransform.position;
                repathTimer = 0f;
            }
        }
    }
}