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

    // „Ÿ„Ÿ„Ÿ FIXED: ACCURATE CINEMACHINE 3 API „Ÿ„Ÿ„Ÿ
    private void ResetCameraOrientation()
    {
        if (closetCam == null) return;

        // 1. Align the base transform to match the prop's forward orientation
        closetCam.transform.rotation = transform.rotation;

        // 2. Grab PanTilt and directly zero out the cached look axes
        CinemachinePanTilt panTilt = closetCam.GetComponent<CinemachinePanTilt>();
        if (panTilt != null)
        {
            // Snaps the horizontal yaw straight forward relative to the prop
            panTilt.PanAxis.Value = 0f;

            // Snaps the vertical pitch perfectly level
            panTilt.TiltAxis.Value = 0f;
        }
    }

    public IEnumerator GoInsideCloset_CO()
    {
        if (isTransitioning || InsideCloset) yield break;

        if (player == null || playerRefs == null)
            FindPlayerReferences();

        if (player == null || playerRefs == null || playerRefs.playerCam == null || closetCam == null)
            yield break;

        isTransitioning = true;

        // --- SNAP CAMERA ORIENTATION HERE ---
        ResetCameraOrientation();

        closetCam.Priority = 100;
        playerRefs.playerCam.Priority = 10;

        // Freeze physics immediately upon entering
        if (playerRefs.rb != null)
        {
            playerRefs.rb.linearVelocity = Vector3.zero;
            playerRefs.rb.angularVelocity = Vector3.zero;
            playerRefs.rb.isKinematic = true;
        }

        // Disable the player collider
        if (playerRefs.playerCollider != null)
        {
            playerRefs.playerCollider.enabled = false;
        }

        // Disable movement
        if (playerRefs.movementScript != null)
            playerRefs.movementScript.enabled = false;

        // Disable player look so mouse inputs don't fight the reset
        if (playerRefs.playerLook != null)
        {
            playerRefs.playerLook.enabled = false;
        }

        // Ensure flashlight is fully off
        if (playerRefs.flashlightScript != null)
        {
            playerRefs.flashlightScript.enabled = false;
        }

        if (playerRefs.bodyMeshRenderer != null)
            playerRefs.bodyMeshRenderer.enabled = false;

        InsideCloset = true;

        // Activate the target dummy object for the stalker to find
        if (stalkerFollowTarget != null)
        {
            stalkerFollowTarget.SetActive(true);
        }

        if (closetAnim != null)
            closetAnim.SetInteger("C", 1);

        yield return new WaitForSeconds(1f);

        if (closetAnim != null)
            closetAnim.SetInteger("C", 0);

        isTransitioning = false;
    }

    public IEnumerator GoOutsideCloset_CO()
    {
        if (isTransitioning || !InsideCloset) yield break;

        if (player == null || playerRefs == null)
            FindPlayerReferences();

        if (player == null || playerRefs == null || playerRefs.playerCam == null || closetCam == null)
            yield break;

        isTransitioning = true;

        if (playerRefs.rb != null)
        {
            playerRefs.rb.isKinematic = true;
        }

        player.position = exitPoint.position;
        player.rotation = exitPoint.rotation;

        playerRefs.playerCam.Priority = 100;
        closetCam.Priority = 10;

        InsideCloset = false;

        // Deactivate the target dummy object
        if (stalkerFollowTarget != null)
        {
            stalkerFollowTarget.SetActive(false);
        }

        if (closetAnim != null)
            closetAnim.SetInteger("C", 1);

        yield return new WaitForSeconds(1f);

        if (closetAnim != null)
            closetAnim.SetInteger("C", 0);

        // Re-enable movement
        if (playerRefs.movementScript != null)
            playerRefs.movementScript.enabled = true;

        // Re-enable player look controls
        if (playerRefs.playerLook != null)
        {
            playerRefs.playerLook.enabled = true;
        }

        // Enable flashlight script back on
        if (playerRefs.flashlightScript != null)
            playerRefs.flashlightScript.enabled = true;

        if (playerRefs.bodyMeshRenderer != null)
            playerRefs.bodyMeshRenderer.enabled = true;

        // Re-enable the player collider
        if (playerRefs.playerCollider != null)
        {
            playerRefs.playerCollider.enabled = true;
        }

        // Unfreeze physics safely
        if (playerRefs.rb != null)
        {
            playerRefs.rb.isKinematic = false;
            playerRefs.rb.linearVelocity = Vector3.zero;
            playerRefs.rb.angularVelocity = Vector3.zero;
        }

        isTransitioning = false;
    }
}