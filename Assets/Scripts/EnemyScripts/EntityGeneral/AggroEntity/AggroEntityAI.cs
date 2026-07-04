using UnityEngine;
using UnityEngine.AI;

public class AggroEntityAI : MonoBehaviour
{
    public Transform movePositionTransform;
    private NavMeshAgent navMeshAgent;

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
            navMeshAgent.destination = movePositionTransform.position;
        }
    }
}