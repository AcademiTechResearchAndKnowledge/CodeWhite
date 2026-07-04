using UnityEngine;

public class PlayerGazeRaycast : MonoBehaviour
{
    [Header("Gaze Settings")]
    [Tooltip("How close the player needs to be for the entity to jumpscare them.")]
    public float gazeDistance = 3f;

    [Tooltip("Recommended: Set this to the layer your items/entities are on so the raycast ignores walls.")]
    public LayerMask interactableLayer;

    void Update()
    {
        // Shoot a invisible line perfectly forward from the center of the camera
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // If the ray hits something within our distance limit...
        if (Physics.Raycast(ray, out hit, gazeDistance, interactableLayer))
        {
            // Check if the object we hit has the JumpscareEntityTrigger script
            JumpscareEntityTrigger entity = hit.collider.GetComponent<JumpscareEntityTrigger>();

            // If it does, fire the scare!
            if (entity != null)
            {
                entity.TriggerScare();
            }
        }
    }
}