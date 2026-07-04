using UnityEngine;
using System.Collections;

public class PhoneButton : MonoBehaviour
{
    public int number;
    public NokiaPuzzle puzzle;

    public AudioSource audioSource;
    public AudioClip pressSound;
    public float soundDuration = 0.1f;

    public void Interact()
    {
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
            StartCoroutine(StopSoundAfterTime(soundDuration));
        }

        puzzle.PressNumber(number);
    }

    IEnumerator StopSoundAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        audioSource.Stop();
    }
}