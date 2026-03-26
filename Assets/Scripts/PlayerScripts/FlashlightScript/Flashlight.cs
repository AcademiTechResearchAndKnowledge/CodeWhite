using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private Light torchLight;
    [SerializeField] private float flickerDuration = 2f;
    [SerializeField] private float minFlickerInterval = 0.05f;
    [SerializeField] private float maxFlickerInterval = 0.15f;

    private Coroutine flickerRoutine;

    private void OnEnable()
    {
        EntityManager.onWhisperFlicker += Flicker;
    }

    private void OnDisable()
    {
        EntityManager.onWhisperFlicker -= Flicker;
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
        }

        if (Keyboard.current.uKey.wasPressedThisFrame)
        {
            Flicker();
        }
    }

    public void Flicker()
    {
        if (flickerRoutine != null)
        {
            StopCoroutine(flickerRoutine);
        }

        if(torchLight.enabled)
        {
            flickerRoutine = StartCoroutine(FlickerRoutine());
        }
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