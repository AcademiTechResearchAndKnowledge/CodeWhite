using UnityEngine;

public class JumpscareEntityTrigger : MonoBehaviour
{
    private JumpscareMechanic jumpscareMechanic;
    private bool hasTriggered = false;

    void Start()
    {
        // Unity 6 standard: Automatically finds the script in your scene
        jumpscareMechanic = FindFirstObjectByType<JumpscareMechanic>();

        if (jumpscareMechanic == null)
        {
            Debug.LogError("No JumpscareMechanic found in the scene! The entity cannot trigger the canvas.");
        }
    }

    // The player's raycast will call this method when looking at the entity
    public void TriggerScare()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        // 1. Activate the UI canvas jumpscare
        if (jumpscareMechanic != null)
        {
            jumpscareMechanic.TriggerJumpscare();
        }

        // 2. Destroy the 3D entity so it vanishes from the drawer
        Destroy(gameObject);
    }
}