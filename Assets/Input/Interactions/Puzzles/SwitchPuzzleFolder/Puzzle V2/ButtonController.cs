using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public static ButtonController Instance;

    [Header("Button GameObjects (index 0, 1, 2)")]
    public GameObject[] buttonObjects;
    public Material buttonOffMaterial;
    public Material buttonOnMaterial;
    public Material buttonLockedMaterial;

    [Header("Single door between both rooms")]
    public GameObject door;
    public Vector3 doorOpenLocalPosition;
    public Vector3 doorClosedLocalPosition;
    public bool useDoorRotation = false;
    public Vector3 doorOpenLocalRotation;
    public Vector3 doorClosedLocalRotation;

    [Header("Bulb")]
    public Light bulbLight;
    public Renderer bulbRenderer;
    public Material bulbOffMaterial;
    public Material bulbOnMaterial;

    private int firstPressedButton = -1;
    private int secondPressedButton = -1;
    private int pressCount = 0;
    private bool buttonsLocked = false;
    private bool doorIsOpen;

    void Awake()
    {
        Instance = this;
        ApplyDoorState(true);
    }

    public void OnButtonPressed(int buttonIndex)
    {
        if (buttonsLocked) return;
        if (pressCount >= 2) return;

        if (pressCount == 0)
        {
            // --- First press ---
            firstPressedButton = buttonIndex;
            SetButtonVisual(buttonIndex, true);

            // Door closes on first press
            doorIsOpen = false;
            ApplyDoorState(false);

            // Light bulb if this first button is correct
            bool firstIsCorrect = SwitchPuzzleManager.Instance.CheckAnswer(buttonIndex);
            SetBulbState(firstIsCorrect);

            // Tell BulbInteraction whether first press was correct
            BulbInteraction.Instance.SetFirstButtonCorrect(firstIsCorrect);

            pressCount = 1;
        }
        else if (pressCount == 1)
        {
            // --- Second press ---
            // Can't press the same button twice
            if (buttonIndex == firstPressedButton) return;

            secondPressedButton = buttonIndex;
            SetButtonVisual(buttonIndex, true);

            // Door opens on second press
            doorIsOpen = true;
            ApplyDoorState(true);

            // Check if the second button pressed is the correct one
            bool secondIsCorrect = SwitchPuzzleManager.Instance.CheckAnswer(buttonIndex);
            SetBulbState(secondIsCorrect);

            // Update BulbInteraction — hot/cold now reflects
            // whichever of the two pressed buttons was correct
            bool eitherWasCorrect = BulbInteraction.Instance.IsFirstButtonCorrect() || secondIsCorrect;
            BulbInteraction.Instance.SetSecondButtonCorrect(secondIsCorrect);

            pressCount = 2;

            // Lock all buttons
            LockAllButtons();
        }
    }

    public void UnlockButtons()
    {
        buttonsLocked = false;
        pressCount = 0;
        firstPressedButton = -1;
        secondPressedButton = -1;

        for (int i = 0; i < buttonObjects.Length; i++)
            SetButtonVisual(i, false);

        SetBulbState(false);
        BulbInteraction.Instance.Reset();
    }

    void LockAllButtons()
    {
        buttonsLocked = true;
        for (int i = 0; i < buttonObjects.Length; i++)
        {
            Renderer r = buttonObjects[i].GetComponent<Renderer>();
            if (r != null && buttonLockedMaterial != null)
                r.material = buttonLockedMaterial;
        }
    }

    public void OnCorrectAnswerGiven()
    {
        UnlockButtons();
    }

    void SetButtonVisual(int index, bool on)
    {
        Renderer r = buttonObjects[index].GetComponent<Renderer>();
        if (r != null)
            r.material = on ? buttonOnMaterial : buttonOffMaterial;
    }

    void SetBulbState(bool on)
    {
        if (bulbLight != null) bulbLight.enabled = on;
        if (bulbRenderer != null)
            bulbRenderer.material = on ? bulbOnMaterial : bulbOffMaterial;
    }

    void ApplyDoorState(bool open)
    {
        if (useDoorRotation)
            door.transform.localEulerAngles = open ? doorOpenLocalRotation : doorClosedLocalRotation;
        else
            door.transform.localPosition = open ? doorOpenLocalPosition : doorClosedLocalPosition;
    }
}