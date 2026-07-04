using UnityEngine;
using UnityEngine.UI;

public class SubmitAnswerButton : MonoBehaviour
{
    public Vector3 flippedRotation = new Vector3(-45, 0, 0);
    public Vector3 defaultRotation = Vector3.zero;

    private bool isFlipped = false;

    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnSubmit);
    }

    public void OnSubmit()
    {
        var handler = SwitchPuzzleHandler.Instance;

        if (handler == null)
        {
            Debug.LogError("SwitchPuzzleHandler.Instance is null!");
            return;
        }

        if (!isFlipped)
        {
            transform.localEulerAngles = flippedRotation;
            isFlipped = true;
        }

        handler.SubmitPuzzle();

        transform.localEulerAngles = defaultRotation;
        isFlipped = false;
    }
}