using UnityEngine;

public class LIBentityHandler : MonoBehaviour
{
    public LIBentity ent_AI;
    public EntityWandering ent_WANDER;
    public PlayerMovement player_MOVEMENT;

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
