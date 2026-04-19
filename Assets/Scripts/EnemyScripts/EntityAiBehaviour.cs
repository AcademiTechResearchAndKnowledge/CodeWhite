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

    [System.Obsolete]
    private void Update()
    {
        if (isResetting) return;

        if (isActive && playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist <= catchDistance)
            {
                if (DT != null)
                {
                    DT.TriggerDialogue();
                }
                    
                detector.ResetPlayerAndNPCs();
            }
        }
        else if (!isActive && agent != null && Vector3.Distance(transform.position, returnPoint.position) > 0.1f)
        {
            agent.SetDestination(returnPoint.position);
        }
    }

    public void Activate()
    {
        isActive = false;
        playerTarget = null;
    }

    public void StartChase(Transform player)
    {
        if (isResetting) return;
        playerTarget = player;
        isActive = true;
    }

    public void ResetNPC()
    {
        isResetting = true;
        isActive = false;
        playerTarget = null;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(returnPoint.position); 
        }

        isResetting = false;
    }
}