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

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip turnOnSound;
    [SerializeField] private AudioClip turnOffSound;
    [SerializeField] private AudioClip flickerSound;

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
                PlaySound(turnOnSound);
                onFlashlightOn?.Invoke();
            }
            else
            {
                PlaySound(turnOffSound);
                onFlashlightOff?.Invoke();
            }
        }
    }

    public void Flicker()
    {
        // --- ADDED: Only flicker if the flashlight is actually turned on ---
        if (torchLight == null || !torchLight.enabled)
        {
            return;
        }

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
            PlaySound(flickerSound);

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
            PlaySound(turnOffSound);
            onFlashlightOff?.Invoke();
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}