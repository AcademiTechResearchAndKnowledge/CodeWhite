using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class ClosetHidingSystem : MonoBehaviour
{
    public CinemachineCamera closetCam;
    public Transform exitPoint;
    public Animator closetAnim;

    [Header("Stalker Targeting")]
    [Tooltip("Assign the empty GameObject positioned outside the closet door here.")]
    public GameObject stalkerFollowTarget;

    private Transform player;
    private PlayerReferences playerRefs;

    public bool InsideCloset = false;
    public bool isTransitioning = false;
    public static ClosetHidingSystem ActiveCloset { get; private set; }

    // --- NEW FLAG: Tells the exit routine if we are being kicked out by the stalker ---
    public bool wasJumpscared = false;

    void Start()
    {
        FindPlayerReferences();

        if (playerRefs != null && playerRefs.playerCam != null && closetCam != null)
        {
            playerRefs.playerCam.Priority = 100;
            closetCam.Priority = 10;
        }

        if (stalkerFollowTarget != null)
        {
            stalkerFollowTarget.SetActive(false);
        }
    }

    void FindPlayerReferences()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("No Player tagged object found.");
            return;
        }

        player = playerObject.transform;
        playerRefs = playerObject.GetComponent<PlayerReferences>();

        if (playerRefs == null)
        {
            Debug.LogError("PlayerReferences is missing on the player.");
            return;
        }

        if (playerRefs.playerCam == null)
        {
            Debug.LogError("playerCam is not assigned in PlayerReferences.");
        }
    }

    private void ResetCameraOrientation()
    {
        if (closetCam == null) return;

        closetCam.transform.rotation = transform.rotation;

        CinemachinePanTilt panTilt = closetCam.GetComponent<CinemachinePanTilt>();
        if (panTilt != null)
        {
            panTilt.PanAxis.Value = 0f;
            panTilt.TiltAxis.Value = 0f;
        }
    }

    public IEnumerator GoInsideCloset_CO()
    {
        if (isTransitioning || InsideCloset) yield break;

        if (player == null || playerRefs == null) FindPlayerReferences();
        if (player == null || playerRefs == null || playerRefs.playerCam == null || closetCam == null) yield break;

        isTransitioning = true;
        ResetCameraOrientation();

        closetCam.Priority = 100;
        playerRefs.playerCam.Priority = 10;

        if (playerRefs.rb != null)
        {
            playerRefs.rb.linearVelocity = Vector3.zero;
            playerRefs.rb.angularVelocity = Vector3.zero;
            playerRefs.rb.isKinematic = true;
        }

        if (playerRefs.playerCollider != null) playerRefs.playerCollider.enabled = false;
        if (playerRefs.movementScript != null) playerRefs.movementScript.enabled = false;
        if (playerRefs.playerLook != null) playerRefs.playerLook.enabled = false;
        if (playerRefs.flashlightScript != null) playerRefs.flashlightScript.enabled = false;
        if (playerRefs.bodyMeshRenderer != null) playerRefs.bodyMeshRenderer.enabled = false;

        InsideCloset = true;
        ActiveCloset = this;

        if (stalkerFollowTarget != null) stalkerFollowTarget.SetActive(true);

        if (closetAnim != null) closetAnim.SetInteger("C", 1);
        yield return new WaitForSeconds(1f);
        if (closetAnim != null) closetAnim.SetInteger("C", 0);

        isTransitioning = false;
    }

    public IEnumerator GoOutsideCloset_CO()
    {
        if (isTransitioning || !InsideCloset) yield break;

        if (player == null || playerRefs == null) FindPlayerReferences();
        if (player == null || playerRefs == null || playerRefs.playerCam == null || closetCam == null) yield break;

        isTransitioning = true;

        if (playerRefs.rb != null) playerRefs.rb.isKinematic = true;

        player.position = exitPoint.position;
        player.rotation = exitPoint.rotation;

        playerRefs.playerCam.Priority = 100;
        closetCam.Priority = 10;

        InsideCloset = false;
        if (ActiveCloset == this) ActiveCloset = null;

        if (stalkerFollowTarget != null) stalkerFollowTarget.SetActive(false);

        // „Ÿ„Ÿ„Ÿ THE FIX: SMART DOOR CLOSING „Ÿ„Ÿ„Ÿ
        if (wasJumpscared)
        {
            // The stalker already opened and unpaused the door. 
            // Skip the "Open" command entirely and just tell it to close!
            if (closetAnim != null) closetAnim.SetInteger("C", 0);

            yield return new WaitForSeconds(1f);

            wasJumpscared = false; // Reset the flag for next time
        }
        else
        {
            // Standard Player Exit: Open the door, wait, then close the door
            if (closetAnim != null) closetAnim.SetInteger("C", 1);
            yield return new WaitForSeconds(1f);
            if (closetAnim != null) closetAnim.SetInteger("C", 0);
        }

        // Re-enable all player controls
        if (playerRefs.movementScript != null) playerRefs.movementScript.enabled = true;
        if (playerRefs.playerLook != null) playerRefs.playerLook.enabled = true;
        if (playerRefs.flashlightScript != null) playerRefs.flashlightScript.enabled = true;
        if (playerRefs.bodyMeshRenderer != null) playerRefs.bodyMeshRenderer.enabled = true;
        if (playerRefs.playerCollider != null) playerRefs.playerCollider.enabled = true;

        if (playerRefs.rb != null)
        {
            playerRefs.rb.isKinematic = false;
            playerRefs.rb.linearVelocity = Vector3.zero;
            playerRefs.rb.angularVelocity = Vector3.zero;
        }

        isTransitioning = false;
    }

    // „Ÿ„Ÿ„Ÿ STALKER JUMPSCARE INTEGRATION METHODS „Ÿ„Ÿ„Ÿ

    public void ForceOpenDoorsForJumpscare()
    {
        wasJumpscared = true; // Tell the exit script that a jumpscare is happening!
        StartCoroutine(JumpscareDoorHoldRoutine());
    }

    private IEnumerator JumpscareDoorHoldRoutine()
    {
        if (closetAnim != null)
        {
            closetAnim.SetInteger("C", 1);
            yield return new WaitForSeconds(0.5f);
            closetAnim.speed = 0f; // Freeze it open
        }
    }

    public void ForceExitCloset()
    {
        if (!InsideCloset) return;

        if (closetAnim != null)
        {
            closetAnim.speed = 1f; // Unfreeze the animator so the GoOutside routine can close it
        }

        ClosetHideInteract interactScript = GetComponent<ClosetHideInteract>();
        if (interactScript != null)
        {
            interactScript.ForceKickedOutByStalker();
        }
        else
        {
            isTransitioning = false;
            StartCoroutine(GoOutsideCloset_CO());
        }
    }
}