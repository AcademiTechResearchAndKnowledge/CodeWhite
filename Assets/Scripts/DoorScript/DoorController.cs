using UnityEngine;
using System.Collections;

public class DoorController : Interactable
{
    public Animator doorAnimator;
    public PortalDoor portal; // ✅ NEW

    public float openTime = 1f;
    public float closeTime = 1f;

    public float autoCloseDelay = 3f;

    // REMOVED: private bool isBusy = false;
    private bool locked;

    private enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    private DoorState state = DoorState.Closed;

    private Coroutine autoCloseCoroutine;
    private int aiInZone = 0;

    public override void Interact()
    {
        if (locked || doorAnimator == null) return;


        if (portal != null && !portal.CanInteractWithDoor())
            return;

        CancelAutoClose();

        if (state == DoorState.Closed)
        {
            StartCoroutine(OpenDoorRoutine());
        }
        else if (state == DoorState.Open)
        {
            StartCoroutine(CloseRoutine());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<WhiteLady>() != null)
        {
            aiInZone++;


            if (portal != null && !portal.CanInteractWithDoor())
                return;

            if (state == DoorState.Closed && !locked)
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

            if (aiInZone <= 0 && state == DoorState.Open && !locked)
            {
                StartCoroutine(CloseRoutine());
            }
        }
    }

    IEnumerator OpenDoorRoutine()
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

    IEnumerator CloseRoutine()
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

    IEnumerator ForceCloseRoutine()
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

    public void OpenFromPortal()
    {
        if (state == DoorState.Closed)
            StartCoroutine(OpenDoorRoutine());
    }
}