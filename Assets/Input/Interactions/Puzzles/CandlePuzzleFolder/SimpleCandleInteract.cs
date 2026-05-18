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

    private void OnEnable()
    {
        WhispererManager.onWhispererSpawned += ForceTurnOff;
    }

    private void OnDisable()
    {
        WhispererManager.onWhispererSpawned -= ForceTurnOff;
    }

    public override void Interact()
    {
        // --- ADDED: Block turning the candle ON if Whisperer is active ---
        if (WhispererManager.IsWhispererActive && !isLit)
        {
            return; // Do nothing, the candle stays off.
        }

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

    private void ForceTurnOff()
    {
        if (isLit)
        {
            isLit = false;
            flame.SetActive(false);

            if (audioSource != null && turnOffSound != null)
            {
                audioSource.PlayOneShot(turnOffSound);
            }
        }
    }
}