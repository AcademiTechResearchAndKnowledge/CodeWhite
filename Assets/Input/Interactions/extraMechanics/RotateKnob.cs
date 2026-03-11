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

    float currentAngle = 0f;
    public bool dragging = false;

    float lastMouseAngle;

    Camera cam;

    void Start()
{
    cam = Camera.main;

    // Initialize knob at minFrequency
    currentAngle = minAngle;
    transform.localRotation = Quaternion.AngleAxis(currentAngle, Vector3.right);

    frequency = minFrequency;
}

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckKnobClick();
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }

        if (dragging)
        {
            RotateKnob();
        }
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
        Vector3 knobScreenPos = cam.WorldToScreenPoint(transform.position);

        Vector2 dir = mousePos - (Vector2)knobScreenPos;

        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }

    public void RotateKnob()
    {
        float mouseAngle = GetMouseAngle();

        float delta = Mathf.DeltaAngle(lastMouseAngle, mouseAngle);

        currentAngle -= delta;
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);

        lastMouseAngle = mouseAngle;

        transform.localRotation = Quaternion.AngleAxis(currentAngle, Vector3.right);

        frequency = Mathf.Lerp(
            minFrequency,
            maxFrequency,
            Mathf.InverseLerp(minAngle, maxAngle, currentAngle)
        );

       // Debug.Log("Frequency: " + frequency.ToString("F1") + " MHz");
    }
}