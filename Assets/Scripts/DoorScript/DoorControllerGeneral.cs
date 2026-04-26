using UnityEngine;
using System.Collections;

public class DoorControllerGeneral : Interactable
{
    [Header("Animation")]
    public Animator doorAnimator;

    [Tooltip("How long to wait before switching to the Idle state after opening/closing.")]
    public float animationStepDelay = 0.5f;

    private bool isOpen = false;
    private bool isBusy = false; // Prevents spamming the interaction key

    // Keeps track of if the White Lady is standing in the doorway
    private int aiInZone = 0;

    // NEW: Reference to the tutorial manager
    private TutorialManager tutorialManager;

    private void Awake()
    {
        // Auto-find the TutorialManager in the scene so you don't have to drag-and-drop it manually
        tutorialManager = FindAnyObjectByType<TutorialManager>();
    }

    public override void Interact()
    {
        // If the door is currently mid-animation, ignore the interaction
        if (isBusy)
            return;

        base.Interact();

        // NEW: Tell the Tutorial Manager we interacted with the door!
        if (tutorialManager != null)
        {
            tutorialManager.DoorInteracted();
        }

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

    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    //  AI Trigger Detector (Automatic Doors)
    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object stepping into the zone is the White Lady
        if (other.GetComponent<WhiteLady>() != null)
        {
            aiInZone++;

            // If the door is currently closed and not moving, open it for her!
            if (!isOpen && !isBusy)
            {
                StartCoroutine(OpenDoorRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<WhiteLady>() != null)
        {
            aiInZone--;

            // If she completely left the zone, and the door is open, close it behind her!
            if (aiInZone <= 0 && isOpen && !isBusy)
            {
                StartCoroutine(CloseDoorRoutine());
            }
        }
    }

    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    //  Coroutines
    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

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