using UnityEngine;
using UnityEngine.InputSystem;

public class LaptopAnswerButton : MonoBehaviour
{
    public int answerIndex = 0;
    public objectZoom zoomScript;

    void Update()
    {
        if (zoomScript == null || !zoomScript.isInPuzzle) return;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    Debug.Log("Clicked answer: " + answerIndex);
                    LaptopManager.Instance.OnAnswerSelected(answerIndex);
                }
            }
        }
    }
}