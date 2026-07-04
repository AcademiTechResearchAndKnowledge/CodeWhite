using UnityEngine;

public class JumpscareEntityTrigger : MonoBehaviour
{
    [Header("Canvas Jumpscare Setup")]
    [Tooltip("Drag the specific Jumpscare Canvas PREFAB for this entity here.")]
    [SerializeField] private JumpscareMechanic jumpscarePrefab;

    private bool hasTriggered = false;

    // The player's raycast (or gaze logic) calls this method
    public void TriggerScare()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        // 1. Spawn and trigger the specific UI jumpscare prefab
        if (jumpscarePrefab != null)
        {
            JumpscareMechanic spawnedJumpscare = Instantiate(jumpscarePrefab);
            spawnedJumpscare.TriggerJumpscare();
        }
        else
        {
            Debug.LogWarning("No Jumpscare Prefab assigned to " + gameObject.name);
        }

        // 2. Destroy the 3D entity so it vanishes from the world
        Destroy(gameObject);
    }
}