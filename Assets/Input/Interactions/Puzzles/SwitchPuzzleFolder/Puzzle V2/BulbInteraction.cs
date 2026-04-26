using UnityEngine;
using System.Collections; // Required for the Coroutine

// FOR THE BULB OBJECT
public class BulbInteraction : Interactable
{
    public static BulbInteraction Instance;

    private bool firstButtonWasCorrect = false;
    private bool secondButtonWasCorrect = false;

    // WE DELETED Awake() HERE!
    // By not having an Awake() method in this script, we allow Unity to successfully 
    // run the Awake() method in your base Interactable.cs script to find the outline.

    void Start()
    {
        // Set up the Singleton instance here instead
        Instance = this;

        // Start the timer to shut down the aggressive QuickOutline script
        StartCoroutine(KillOutlineAfterSetup());
    }

    private IEnumerator KillOutlineAfterSetup()
    {
        // Wait exactly 1 frame for QuickOutline to finish throwing its tantrum
        yield return new WaitForEndOfFrame();

        // Safely turn it off. This now works because the base script successfully found the outline!
        DisableOutline();
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

        if (firstButtonWasCorrect || secondButtonWasCorrect)
            LaptopManager.Instance.ShowHint("The bulb is HOT — one of your pressed buttons is connected!");
        else
            LaptopManager.Instance.ShowHint("The bulb is COLD — neither button you pressed was connected.");
    }
}