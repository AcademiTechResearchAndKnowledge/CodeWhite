using UnityEngine;

public class CHURentityHandler : MonoBehaviour
{
    public CHURentity ent_AI;
    public EntityWondering ent_WANDER;
    public PlayerMovement player_MOVEMENT;
    public void Awake()
    {
       player_MOVEMENT = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }
    public void Update()
    {
        if(player_MOVEMENT.isCrouching)
        {
            ent_WANDER.enabled = true;
            ent_AI.enabled = false;
        }
        else
        {
            ent_WANDER.enabled = false;
            ent_AI.enabled = true;
        }
    }
}
