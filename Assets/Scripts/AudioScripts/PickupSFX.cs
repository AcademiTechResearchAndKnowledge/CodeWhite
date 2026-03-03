using UnityEngine;

public class PickupSFX : MonoBehaviour
{
    [SerializeField] private AudioClip pickupClip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    public void Play()
    {
        if (pickupClip == null) return;

        // Plays even if this item gets destroyed by another script right after
        AudioSource.PlayClipAtPoint(pickupClip, transform.position, volume);
    }
}