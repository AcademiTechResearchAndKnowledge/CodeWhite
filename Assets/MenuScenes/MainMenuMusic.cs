using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    // Call this when the mouse enters the button area
    public void PlayHoverSound()
    {
        sfxSource.PlayOneShot(hoverSound);
    }

    // Call this when the button is actually clicked
    public void PlayClickSound()
    {
        sfxSource.PlayOneShot(clickSound);
    }
}