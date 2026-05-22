using UnityEngine;

public class HandVisualSync : MonoBehaviour
{
    [Tooltip("Drag your actual Main Camera here (NOT the VCam)")]
    public Transform mainCamera;
    public Vector3 positionOffset;

    void LateUpdate()
    {
        if (mainCamera == null) return;

        // Force the hand to snap exactly to the lens rotation
        transform.rotation = mainCamera.rotation;

        // Snap to the position, respecting your offset relative to where the camera is looking
        transform.position = mainCamera.position + (mainCamera.rotation * positionOffset);
    }
}