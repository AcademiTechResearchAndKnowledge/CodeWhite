using UnityEngine;
using UnityEngine.InputSystem;

public class AnalogClock : MonoBehaviour
{
    public Transform hourPivot;
    public Transform minutePivot;
    public int targetHour = 1;
    public int targetMinute = 30;
    public Camera mainCamera;
    public Collider hourCollider;
    public Collider minuteCollider;

    private bool draggingHour = false;
    private bool draggingMinute = false;
    private int hours = 12;
    private int minutes = 0;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        UpdateClockVisuals();
    }

    void Update()
    {
        HandleDragging();
    }

    private void HandleDragging()
    {
        if (Mouse.current == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider == hourCollider)
                {
                    draggingHour = true;
                    draggingMinute = false;
                }
                else if (hit.collider == minuteCollider)
                {
                    draggingHour = false;
                    draggingMinute = true;
                }
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            draggingHour = false;
            draggingMinute = false;
        }

        if (draggingHour || draggingMinute)
        {
            Plane rotationPlane = new Plane(Vector3.forward, draggingHour ? hourPivot.position : minutePivot.position);
            if (rotationPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Transform hand = draggingHour ? hourPivot : minutePivot;
                Vector3 dir = hitPoint - hand.position;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

                if (draggingHour)
                    angle = Mathf.Round(angle / 30f) * 30f;
                else
                    angle = Mathf.Round(angle / 6f) * 6f;

                hand.localRotation = Quaternion.Euler(0, 0, angle);
                UpdateTimeFromHands();
            }
        }
    }

    private void UpdateTimeFromHands()
    {
        float minuteRot = minutePivot.localEulerAngles.z;
        minutes = Mathf.RoundToInt(minuteRot / 6f) % 60;

        float hourRot = hourPivot.localEulerAngles.z;
        hours = Mathf.RoundToInt(hourRot / 30f) % 12;
        if (hours == 0) hours = 12;



        CheckPuzzle();
    }

    private void CheckPuzzle()
    {
        if (hours == targetHour && minutes == targetMinute)
            Debug.Log("Puzzle Done");
    }

    private void UpdateClockVisuals()
    {
        float minuteRotation = minutes * 6f;
        float hourRotation = (hours % 12) * 30f + minutes * 0.5f;
        minutePivot.localRotation = Quaternion.Euler(0, 0, minuteRotation);
        hourPivot.localRotation = Quaternion.Euler(0, 0, hourRotation);
        Debug.Log($"Time: {hours:00}:{minutes:00} | HourRot: {hourRotation:F1} | MinuteRot: {minuteRotation:F1}");
    }
}