using UnityEngine;
using System.Collections;

// FOR THE BULB OBJECT
public class BulbInteraction : Interactable
{
    public static BulbInteraction Instance;

    [Header("Dialogue Settings")]
    [Tooltip("The ID of the dialogue to play when the bulb is HOT.")]
    public string hotDialogueID = "BulbHot";
    [Tooltip("The ID of the dialogue to play when the bulb is COLD.")]
    public string coldDialogueID = "BulbCold";

    private bool firstButtonWasCorrect = false;
    private bool secondButtonWasCorrect = false;

    void Start()
    {
        Instance = this;

        StartCoroutine(KillOutlineAfterSetup());
    }

    private IEnumerator KillOutlineAfterSetup()
    {
        yield return new WaitForEndOfFrame();

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
        {
            LaptopManager.Instance.ShowHint("The bulb is HOT — one of your pressed buttons is connected!");

            DialogueData hotData = DialogueDatabase.Instance.GetDialogue(hotDialogueID);
            if (hotData != null)
            {
                DialogueManager.Instance.StartDialogue(hotData);
            }
        }
        else
        {
            LaptopManager.Instance.ShowHint("The bulb is COLD — neither button you pressed was connected.");

            DialogueData coldData = DialogueDatabase.Instance.GetDialogue(coldDialogueID);
            if (coldData != null)
            {
                DialogueManager.Instance.StartDialogue(coldData);
            }
        }
    }
}