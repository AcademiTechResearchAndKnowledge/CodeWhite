using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Flashlight : MonoBehaviour
{
    public delegate void OnFlashlightOn();
    public static event OnFlashlightOn onFlashlightOn;

    public delegate void OnFlashlightOff();
    public static event OnFlashlightOff onFlashlightOff;

    [SerializeField] private float flickerDuration = 2f;
    [SerializeField] private float minFlickerInterval = 0.05f;
    [SerializeField] private float maxFlickerInterval = 0.15f;

    private Coroutine flickerRoutine;
    [SerializeField] private InputActionReference toggleAction;
    [SerializeField] public Light torchLight;

    // --- ADDED: Audio variables ---
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip turnOnSound;
    [SerializeField] private AudioClip turnOffSound;

    private void OnEnable()
    {
        WhispererManager.onWhisperFlicker += Flicker;
        WhispererManager.onWhispererSpawned += ForceTurnOff;
    }

    private void OnDisable()
    {
        WhispererManager.onWhisperFlicker -= Flicker;
        WhispererManager.onWhispererSpawned -= ForceTurnOff;
    }

    private void Awake()
    {
        torchLight.enabled = false;

        // --- ADDED: Auto-grab the AudioSource if it exists on this object but wasn't assigned manually ---
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (WhispererManager.IsWhispererActive && !torchLight.enabled)
            {
                return; // Do nothing, the light stays off.
            }

            torchLight.enabled = !torchLight.enabled;

            if (torchLight.enabled)
            {
                PlaySound(turnOnSound); // --- ADDED ---
                onFlashlightOn?.Invoke();
            }
            else
            {
                PlaySound(turnOffSound); // --- ADDED ---
                onFlashlightOff?.Invoke();
            }
        }
    }

    public void Flicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
        }

        flickerRoutine = StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        float timer = 0f;
        bool originalState = torchLight.enabled;

        while (timer < flickerDuration)
        {
            torchLight.enabled = !torchLight.enabled;

            float waitTime = Random.Range(minFlickerInterval, maxFlickerInterval);
            timer += waitTime;

            yield return new WaitForSeconds(waitTime);
        }

        torchLight.enabled = originalState;
        flickerRoutine = null;
    }

    public void ForceTurnOff()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }

        if (torchLight.enabled)
        {
            torchLight.enabled = false;
            PlaySound(turnOffSound); // --- ADDED ---
            onFlashlightOff?.Invoke();
        }
    }

    // --- ADDED: Helper method to safely play the assigned clips ---
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}