using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchInteract : MonoBehaviour
{
    public int switchID;
    public Transform switchRot;

    public Vector3 flippedRotation = new Vector3(-45, 0, 0);
    public Vector3 defaultRotation = Vector3.zero;

    public bool isFlipped = false;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Interactable") && hit.collider.gameObject == gameObject)
                {
                    ToggleSwitch();
                }
            }
        }
    }

    public void ToggleSwitch()
    {
        if (SwitchPuzzleHandler.Instance == null)
        {
            Debug.LogError("SwitchPuzzleHandler.Instance is null!");
            return;
        }

        if (SwitchPuzzleHandler.Instance.IsSolved())
            return;

        isFlipped = !isFlipped;

        if (switchRot != null)
        {
            switchRot.localEulerAngles = isFlipped ? flippedRotation : defaultRotation;
        }

        SwitchPuzzleHandler.Instance.FlipSwitch(switchID);

        Debug.Log($"Switch {switchID} is now {(isFlipped ? "ON" : "OFF")}");
    }

    public void ResetSwitch()
    {
        isFlipped = false;

        if (switchRot != null)
        {
            switchRot.localEulerAngles = defaultRotation;
        }
    }
}