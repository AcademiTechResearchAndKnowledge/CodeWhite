using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
public class CHURentity : MonoBehaviour
{
    [SerializeField] public Transform movePositionTransform;
    public PlayerMovement player_MOVEMENT;
    private NavMeshAgent navMeshAgent;
    
    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player_MOVEMENT = GameObject.Find("Player").GetComponent<PlayerMovement>();
        movePositionTransform = GameObject.Find("Player").transform;
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
