using UnityEngine;

// Attach this to each of the 3 physical answer objects on the laptop screen.
// Each one represents one answer choice (Button 1, 2, or 3).
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