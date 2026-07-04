using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("References")]
    public CinemachineBobble bobScript; // Drag your Virtual Camera in here!
    public PlayerMovement playerMovement;
    public AudioSource audioSource;
    public AudioClip[] footstepSounds;

    [Header("Dynamic Audio Settings")]
    [Range(0f, 1f)] public float sneakVolume = 0.2f;
    [Range(0f, 1f)] public float walkVolume = 0.5f;
    [Range(0f, 1f)] public float sprintVolume = 0.9f;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    // Subscribe to the event when enabled
    void OnEnable()
    {
        if (bobScript != null)
            bobScript.OnStep += PlayFootstepSound;
    }

    // Unsubscribe when disabled (Crucial to prevent memory leaks!)
    void OnDisable()
    {
        if (bobScript != null)
            bobScript.OnStep -= PlayFootstepSound;
    }

    void PlayFootstepSound()
    {
        if (footstepSounds.Length == 0 || playerMovement == null) return;

        // Check speeds to adjust volume dynamically
        float speed = playerMovement.HorizontalSpeed;
        bool isSprinting = speed > 5f;
        bool isCrouching = speed < 3f;

        float currentVolume = walkVolume;
        if (isSprinting) currentVolume = sprintVolume;
        else if (isCrouching) currentVolume = sneakVolume;

        // Apply slight pitch variation to avoid the machine-gun effect
        audioSource.pitch = Random.Range(0.85f, 1.15f);
        audioSource.volume = currentVolume;

        // Pick a random clip and play
        int randomIndex = Random.Range(0, footstepSounds.Length);
        audioSource.PlayOneShot(footstepSounds[randomIndex]);
    }
}