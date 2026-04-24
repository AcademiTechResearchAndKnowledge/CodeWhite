using UnityEngine;
using System.Collections;

public class DoorController : Interactable
{
    public Animator doorAnimator;

    public float openTime = 1f;
    public float closeTime = 1f;

    // NEW: auto close delay
    public float autoCloseDelay = 3f;

    private enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    private DoorState state = DoorState.Closed;
    private bool locked;


    private Coroutine autoCloseCoroutine;

    // Keeps track of if the White Lady is standing in the doorway
    private int aiInZone = 0;

    public override void Interact()
    {
        if (locked || doorAnimator == null) return;

        // cancel auto-close if player interacts
        CancelAutoClose();

        if (state == DoorState.Closed)
        {
            StartCoroutine(OpenRoutine());
        }
        else if (state == DoorState.Open)
        {
            StartCoroutine(CloseRoutine());
        }
    }

<<<<<<< Updated upstream
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
=======
    private IEnumerator OpenRoutine()
>>>>>>> Stashed changes
    {
        locked = true;
        state = DoorState.Opening;

        doorAnimator.Play("Door - Open");

        yield return new WaitForSeconds(openTime);

        doorAnimator.Play("Door - Open Idle");

        state = DoorState.Open;
        locked = false;


        StartAutoClose();
    }

    private IEnumerator CloseRoutine()
    {
        locked = true;
        state = DoorState.Closing;

        doorAnimator.Play("Door - Close");

        yield return new WaitForSeconds(closeTime);

        doorAnimator.Play("Door - Idle");

        state = DoorState.Closed;
        locked = false;

        CancelAutoClose();
    }

    public void ForceCloseFromPortal()
    {
        StopAllCoroutines();
        CancelAutoClose();
        StartCoroutine(ForceCloseRoutine());
    }

    private IEnumerator ForceCloseRoutine()
    {
        locked = true;
        state = DoorState.Closing;

        doorAnimator.Play("Door - Close");

        yield return new WaitForSeconds(closeTime);

        doorAnimator.Play("Door - Idle");

        state = DoorState.Closed;
        locked = false;
    }



    private void StartAutoClose()
    {
        CancelAutoClose();
        autoCloseCoroutine = StartCoroutine(AutoCloseRoutine());
    }

    private void CancelAutoClose()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    private IEnumerator AutoCloseRoutine()
    {
        yield return new WaitForSeconds(autoCloseDelay);

        if (state == DoorState.Open && !locked)
        {
            StartCoroutine(CloseRoutine());
        }

        autoCloseCoroutine = null;
    }


    public void AutoClose()
    {
        if (state == DoorState.Open && !locked)
        {
            StartCoroutine(CloseRoutine());
        }
    }

    public void OpenFromPortal()
{
    if (state == DoorState.Closed)
        StartCoroutine(OpenRoutine());
}
}