using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class LIBentity : MonoBehaviour
{
    [SerializeField] public Transform movePositionTransform;
    public PlayerMovement player_MOVEMENT;
    private NavMeshAgent navMeshAgent;
    
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {   
        if (player_MOVEMENT.isCrouching == false)
        {
            if(movePositionTransform)
            {
                navMeshAgent.destination = movePositionTransform.position;
            }
        }

    }
}
