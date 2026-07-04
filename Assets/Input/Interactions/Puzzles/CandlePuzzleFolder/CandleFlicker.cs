using System.Collections;
using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    [SerializeField] private Light torchLight;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource flickerAudio;

    [Header("Flicker Settings")]
    [SerializeField] private float flickerDuration = 2f;
    [SerializeField] private float minFlickerInterval = 0.05f;
    [SerializeField] private float maxFlickerInterval = 0.15f;

    private Coroutine flickerRoutine;
    private bool originalLightState;

    private void OnEnable()
    {
        WhispererManager.onWhisperFlicker += Flicker;
    }

    private void OnDisable()
    {
        WhispererManager.onWhisperFlicker -= Flicker;
    }

    public void Flicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
            torchLight.enabled = originalLightState;

            if (flickerAudio != null) flickerAudio.Stop();
        }

        if (torchLight.enabled)
        {
            flickerRoutine = StartCoroutine(FlickerRoutine());
        }
    }

    private IEnumerator FlickerRoutine()
    {
        float timer = 0f;
        originalLightState = torchLight.enabled;

        if (flickerAudio != null)
        {
            flickerAudio.loop = true;
            flickerAudio.Play();
        }

        while (timer < flickerDuration)
        {
            torchLight.enabled = !torchLight.enabled;

            float waitTime = Random.Range(minFlickerInterval, maxFlickerInterval);
            timer += waitTime;

            yield return new WaitForSeconds(waitTime);
        }

        torchLight.enabled = originalLightState;

        if (flickerAudio != null)
        {
            flickerAudio.Stop();
        }

        flickerRoutine = null;
    }
}