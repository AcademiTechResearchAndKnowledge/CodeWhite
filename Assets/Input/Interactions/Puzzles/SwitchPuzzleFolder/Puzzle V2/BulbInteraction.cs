using UnityEngine;

public class BulbInteraction : Interactable
{
    public static BulbInteraction Instance;

    private bool firstButtonWasCorrect = false;
    private bool secondButtonWasCorrect = false;

    void Awake()
    {
        Instance = this;
    }

    public void SetFirstButtonCorrect(bool wasCorrect)
    {
        firstButtonWasCorrect = wasCorrect;
    }

    public void SetSecondButtonCorrect(bool wasCorrect)
    {
        secondButtonWasCorrect = wasCorrect;
    }

    public bool IsFirstButtonCorrect()
    {
        return firstButtonWasCorrect;
    }

    public void Reset()
    {
        firstButtonWasCorrect = false;
        secondButtonWasCorrect = false;
    }

    public override void Interact()
    {
        base.Interact();

        // Hot if either the first or second button pressed was the correct one
        if (firstButtonWasCorrect || secondButtonWasCorrect)
            LaptopManager.Instance.ShowHint("The bulb is HOT — one of your pressed buttons is connected!");
        else
            LaptopManager.Instance.ShowHint("The bulb is COLD — neither button you pressed was connected.");
    }
}