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

    private void OnEnable()
    {
        WhispererManager.onWhisperFlicker += Flicker;
    }

    private void OnDisable()
    {
        WhispererManager.onWhisperFlicker -= Flicker;
    }

    private void Awake()
    {
        torchLight.enabled = false;
    }

    private void Update()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            torchLight.enabled = !torchLight.enabled;

            if (torchLight.enabled)
            {
                onFlashlightOn?.Invoke();
            }
            else
            {
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
}