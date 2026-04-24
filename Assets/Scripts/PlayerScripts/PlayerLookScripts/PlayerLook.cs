using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform body;
    public Transform cameraPivot;

    private float xRotation = 0f;
    private Vector2 lookInput;

    private int ignoreFrames = 5;
    public bool canLook = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!canLook)
            return;

        if (ignoreFrames > 0)
        {
            ignoreFrames--;
            lookInput = Vector2.zero;
            return;
        }

        HandleMouseLook();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void HandleMouseLook()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        body.Rotate(Vector3.up * mouseX);
    }

    public void ForceLookUp(float angle)
    {
        xRotation = angle;
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}