using UnityEngine;
using UnityEngine.AI;

public class EntityAi : MonoBehaviour
{
    // Changed back to public so PlayerActionDetector can access it!
    public Transform movePositionTransform;

    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        // It will still automatically find the player on start if it's currently empty
        if (movePositionTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
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