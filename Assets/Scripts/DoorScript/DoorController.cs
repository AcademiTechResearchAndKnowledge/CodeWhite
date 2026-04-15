using UnityEngine;
using System.Collections;

public class DoorController : Interactable
{
    [Header("Animation")]
    public Animator doorAnimator;

    [Tooltip("How long to wait before switching to the Idle state after opening/closing.")]
    public float animationStepDelay = 0.5f;

    private bool isOpen = false;
    private bool isBusy = false; // Prevents spamming the interaction key

    public override void Interact()
    {
        // If the door is currently mid-animation, ignore the interaction
        if (isBusy)
            return;

        base.Interact();

        if (doorAnimator == null)
        {
            Debug.LogWarning("No Animator assigned on " + gameObject.name);
            return;
        }

        if (!isOpen)
        {
            StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            StartCoroutine(CloseDoorRoutine());
        }
    }

    IEnumerator OpenDoorRoutine()
    {
        isBusy = true;

        // C=1: Opens Door
        doorAnimator.SetInteger("C", 1);

        // Wait for the opening animation to finish
        yield return new WaitForSeconds(animationStepDelay);

        // C=2: Open Door Idle (door stays open)
        doorAnimator.SetInteger("C", 2);

        isOpen = true;
        isBusy = false;
    }

    IEnumerator CloseDoorRoutine()
    {
        isBusy = true;

        // C=3: Close Door
        doorAnimator.SetInteger("C", 3);

        // Wait for the closing animation to finish
        yield return new WaitForSeconds(animationStepDelay);

        // C=4: Goes back to idle (door is closed)
        doorAnimator.SetInteger("C", 4);

        isOpen = false;
        isBusy = false;
    }
}