using UnityEngine;
using UnityEngine.AI;

public class EntityAi : MonoBehaviour
{
    [SerializeField] public Transform movePositionTransform;
    private NavMeshAgent navMeshAgent;
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {   
        if(movePositionTransform)
        {
            navMeshAgent.destination = movePositionTransform.position;
        }
        
    }
}
