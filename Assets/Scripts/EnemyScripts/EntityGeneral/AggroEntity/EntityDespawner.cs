using UnityEngine;

public class EntityDespawner : MonoBehaviour
{
    [Tooltip("The particle effect to spawn when the entity disappears.")]
    public GameObject despawnParticlePrefab;

    // Call this method whenever you want the entity to vanish with style
    public void DespawnWithParticles()
    {
        if (despawnParticlePrefab != null)
        {
            Instantiate(despawnParticlePrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("Despawn Particle Prefab is not assigned on " + gameObject.name);
        }

        // Destroy the GameObject this script is attached to
        Destroy(gameObject);
    }
}