using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class AnalogClock : MonoBehaviour
{
    public Transform hourPivot;
    public Transform minutePivot;
    public Camera mainCamera;
    public Collider hourCollider;
    public Collider minuteCollider;

    private bool draggingHour = false;
    private bool draggingMinute = false;
    public int hours = 3;
    public int minutes = 0;

    public objectZoom objzoom;


    public static List<AnalogClock> allClocks = new List<AnalogClock>();
    public static bool puzzleDone = false;

    void Awake()
    {
        allClocks.Add(this);
    }

    void OnDestroy()
    {
        allClocks.Remove(this);
    }

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        SetPuzzleActive(false);
        UpdateClockVisuals();
    }

    void Update()
    {
        if (objzoom.isInPuzzle)
            SetPuzzleActive(true);
        else
            SetPuzzleActive(false);

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
            Transform hand = draggingHour ? hourPivot : minutePivot;

            Plane rotationPlane = new Plane(transform.forward, hand.position);

            if (rotationPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);

                Vector3 localPoint = transform.InverseTransformPoint(hitPoint);
                Vector3 localCenter = transform.InverseTransformPoint(hand.position);

                Vector3 dir = localPoint - localCenter;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

                if (draggingHour)
                    angle = Mathf.Round(angle / 30f) * 30f;
                else
                    angle = Mathf.Round(angle / 6f) * 6f;

                hand.localRotation = Quaternion.Euler(0f, 0f, angle);

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


        CheckAllClocks();
    }

    private void CheckAllClocks()
    {
        if (puzzleDone) return;

        foreach (AnalogClock clock in allClocks)
        {
            if (clock.hours != 5 || clock.minutes != 0)
                return;
        }

        puzzleDone = true;
        Debug.Log("Puzzle Completed");
    }

    public void SetPuzzleActive(bool state)
    {
        objzoom.isInPuzzle = state;
    }

    public void UpdateClockVisuals()
    {
        float minuteRotation = minutes * 6f;
        float hourRotation = (hours % 12) * 30f + minutes * 0.5f;

        minutePivot.localRotation = Quaternion.Euler(0f, 0f, minuteRotation);
        hourPivot.localRotation = Quaternion.Euler(0f, 0f, hourRotation);
    }
}