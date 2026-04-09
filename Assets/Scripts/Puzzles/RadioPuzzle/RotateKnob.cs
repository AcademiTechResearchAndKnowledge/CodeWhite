using UnityEngine;
using UnityEngine.InputSystem;

public class KnobRotate : MonoBehaviour
{
    public float minAngle = -135f;
    public float maxAngle = 135f;
    public float rotationSpeed = 0.5f;

    public float minFrequency = 88f;
    public float maxFrequency = 108f;

    public Transform pivot;
    public Vector3 rotationOffset = Vector3.zero;

    public Transform pointer;
    public float pointerMinX = -0.5f;
    public float pointerMaxX = 0.5f;

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public float audioMinPitch = 0.5f;
    public float audioMaxPitch = 2f;

    [HideInInspector]
    public float frequency = 88f;

    [HideInInspector]
    public bool dragging = false;
    [HideInInspector]
    public bool submitted = false;

    private float currentAngle = 0f;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        currentAngle = minAngle;
        ApplyRotation();
        frequency = minFrequency;

        if (audioSource != null)
            audioSource.loop = true;
    }

    void Update()
    {
        if (!submitted)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                CheckKnobClick();

            if (Mouse.current.leftButton.wasReleasedThisFrame)
                dragging = false;

            if (dragging)
                RotateKnob();
        }

        UpdateAudio();
    }

    void CheckKnobClick()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == transform || (pivot != null && hit.transform == pivot))
                dragging = true;
        }
    }

    void RotateKnob()
    {
        float delta = Mouse.current.delta.ReadValue().x;
        currentAngle += delta * rotationSpeed;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        ApplyRotation();
        frequency = Mathf.Lerp(minFrequency, maxFrequency,
            Mathf.InverseLerp(minAngle, maxAngle, currentAngle));
    }

    void ApplyRotation()
    {
        if (pivot != null)
            pivot.localRotation = Quaternion.Euler(rotationOffset) * Quaternion.AngleAxis(currentAngle, Vector3.down);
        else
            transform.localRotation = Quaternion.Euler(rotationOffset) * Quaternion.AngleAxis(currentAngle, Vector3.down);

        if (pointer != null)
        {
            float t = Mathf.InverseLerp(minAngle, maxAngle, currentAngle);
            float pointerX = Mathf.Lerp(pointerMinX, pointerMaxX, t);
            Vector3 pos = pointer.localPosition;
            pos.x = pointerX;
            pointer.localPosition = pos;
        }
    }

    void UpdateAudio()
    {
        if (audioSource != null)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();

            audioSource.pitch = Mathf.Lerp(audioMinPitch, audioMaxPitch,
                Mathf.InverseLerp(minFrequency, maxFrequency, frequency));
        }
    }

    public void ResetKnob()
    {
        submitted = false;
        currentAngle = minAngle;
        ApplyRotation();
        frequency = minFrequency;
        UpdateAudio();
    }
}