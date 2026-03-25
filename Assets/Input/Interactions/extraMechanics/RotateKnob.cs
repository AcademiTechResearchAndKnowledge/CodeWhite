using UnityEngine;
using UnityEngine.InputSystem;

public class KnobRotate : MonoBehaviour
{
    [Header("Rotation")]
    public float minAngle = -135f;
    public float maxAngle = 135f;
    public float frequency = 0;

    [Header("Frequency")]
    public float minFrequency = 88f;
    public float maxFrequency = 108f;

    [Header("Custom Pivot")]
    public Transform pivot; // Assign your pivot object in Inspector
                            // If null, falls back to local origin (original behavior)

    float currentAngle = 0f;
    public bool dragging = false;

    float lastMouseAngle;
    Vector3 initialOffset; // Offset from pivot to this object at start

    Camera cam;

    void Start()
    {
        cam = Camera.main;

        // Cache the initial offset from the pivot
        if (pivot != null)
            initialOffset = transform.position - pivot.position;

        // Initialize knob at minFrequency
        currentAngle = minAngle;
        ApplyRotation();

        frequency = minFrequency;
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            CheckKnobClick();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
            dragging = false;

        if (dragging)
            RotateKnob();
    }

    void CheckKnobClick()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                dragging = true;
                lastMouseAngle = GetMouseAngle();
            }
        }
    }

    float GetMouseAngle()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Angle relative to pivot screen position (if set), else object itself
        Vector3 originWorld = pivot != null ? pivot.position : transform.position;
        Vector3 originScreen = cam.WorldToScreenPoint(originWorld);

        Vector2 dir = mousePos - (Vector2)originScreen;
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    public void RotateKnob()
    {
        float mouseAngle = GetMouseAngle();
        float delta = Mathf.DeltaAngle(lastMouseAngle, mouseAngle);

        currentAngle -= delta;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        lastMouseAngle = mouseAngle;

        ApplyRotation();

        frequency = Mathf.Lerp(
            minFrequency,
            maxFrequency,
            Mathf.InverseLerp(minAngle, maxAngle, currentAngle)
        );
    }

    void ApplyRotation()
    {
        if (pivot != null)
        {
            // Orbit around pivot: rotate the initial offset, then apply to pivot position
            Quaternion rot = Quaternion.AngleAxis(currentAngle, Vector3.right);
            transform.position = pivot.position + rot * initialOffset;

            // Also spin the object itself to match (so it faces correctly)
            transform.localRotation = rot;
        }
        else
        {
            // Original behavior — no pivot assigned
            transform.localRotation = Quaternion.AngleAxis(currentAngle, Vector3.right);
        }
    }
}