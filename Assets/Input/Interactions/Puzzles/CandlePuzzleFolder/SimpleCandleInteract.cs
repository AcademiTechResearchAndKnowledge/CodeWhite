using UnityEngine;

public class SimpleCandleInteract : Interactable
{
    public delegate void OnSimpleCandleLit();
    public static event OnSimpleCandleLit onSimpleCandleLit;

    [Header("Visuals")]
    public GameObject flame;
    private bool isLit = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip turnOnSound;
    public AudioClip turnOffSound;

    public override void Interact()
    {
        isLit = !isLit;

        flame.SetActive(isLit);

        if (isLit)
        {
            if (audioSource != null && turnOnSound != null)
            {
                audioSource.PlayOneShot(turnOnSound);
            }

            onSimpleCandleLit?.Invoke();
        }
        else
        {
            if (audioSource != null && turnOffSound != null)
            {
                audioSource.PlayOneShot(turnOffSound);
            }
        }
    }
}