using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InteractionSoundPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip interactionSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [Header("Pitch Randomization (Optional)")]
    [Tooltip("Makes the sound slightly different every time it plays.")]
    [SerializeField] private bool randomizePitch = false;
    [SerializeField, Range(0.1f, 2f)] private float minPitch = 0.9f;
    [SerializeField, Range(0.1f, 2f)] private float maxPitch = 1.1f;

    private AudioSource audioSource;

    private void Awake()
    {
        // Automatically grab the AudioSource on this object
        audioSource = GetComponent<AudioSource>();

        // Ensure it doesn't play automatically when the scene starts
        audioSource.playOnAwake = false;
    }

    // Call this method whenever the object is interacted with
    [ContextMenu("Test Play Sound")]
    public void PlaySound()
    {
        if (interactionSound == null)
        {
            Debug.LogWarning($"InteractionSoundPlayer on {gameObject.name} is missing an Audio Clip!");
            return;
        }

        // Apply pitch randomization if enabled
        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            audioSource.pitch = 1f; // Reset to default
        }

        // PlayOneShot allows the sound to overlap itself if triggered rapidly
        audioSource.PlayOneShot(interactionSound, volume);
    }
}