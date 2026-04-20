using UnityEngine;

// FOR LAPTOP'S BUTTONS OBJECTS
public class LaptopAnswerButton : Interactable
{
    [Header("Which answer index this represents (0, 1, or 2)")]
    public int answerIndex = 0;

    public override void Interact()
    {
        base.Interact();
        LaptopManager.Instance.OnAnswerSelected(answerIndex);
    }
}