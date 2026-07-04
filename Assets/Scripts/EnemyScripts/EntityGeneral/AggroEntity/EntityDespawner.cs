using UnityEngine;

public class EntityDespawner : MonoBehaviour
{
    [Header("Visuals")]
    [Tooltip("The particle effect to spawn when the entity disappears.")]
    public GameObject despawnParticlePrefab;

    [Header("Audio")]
    [Tooltip("The sound to play when the entity despawns (e.g., chase end music).")]
    public AudioClip despawnSound;

    [Tooltip("Volume for the despawn sound.")]
    [Range(0f, 1f)]
    public float despawnVolume = 1.0f;

    // Call this method whenever you want the entity to vanish with style
    public void DespawnWithParticles()
    {
        // 1. Spawn the particles
        if (despawnParticlePrefab != null)
        {
            Instantiate(despawnParticlePrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("Despawn Particle Prefab is not assigned on " + gameObject.name);
        }

        // 2. Play the chase end music/sound safely as 2D Audio
        if (despawnSound != null)
        {
            Play2DAudio(despawnSound, despawnVolume);
        }

        // 3. Destroy the GameObject this script is attached to
        Destroy(gameObject);
    }

    // --- NEW: Custom function to play a 2D sound that outlives this object ---
    private void Play2DAudio(AudioClip clip, float volume)
    {
        // Create an empty GameObject to act as our temporary record player
        GameObject tempAudioObject = new GameObject("TempDespawnAudio");

        // Add an AudioSource component to it
        AudioSource source = tempAudioObject.AddComponent<AudioSource>();

        // Configure it for 2D Sound
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f; // 0 means fully 2D (heard everywhere equally)

        // Play the sound
        source.Play();

        // Tell Unity to destroy this temporary object exactly when the clip finishes playing
        Destroy(tempAudioObject, clip.length);
    }
}