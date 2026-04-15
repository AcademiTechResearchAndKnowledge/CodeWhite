using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class ClosetHidingSystem : MonoBehaviour
{
    public CinemachineCamera closetCam;
    public Transform exitPoint;
    public Animator closetAnim;

    [Header("Stalker Targeting")]
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

    public IEnumerator GoInsideCloset_CO()
    {
        if (isTransitioning || InsideCloset) yield break;

        if (player == null || playerRefs == null)
            FindPlayerReferences();

        if (player == null || playerRefs == null || playerRefs.playerCam == null || closetCam == null)
            yield break;

        isTransitioning = true;

        closetCam.Priority = 100;
        playerRefs.playerCam.Priority = 10;

        if (playerRefs.movementScript != null)
            playerRefs.movementScript.enabled = false;

        if (playerRefs.flashlightScript != null)
            playerRefs.flashlightScript.enabled = false;

        if (playerRefs.bodyMeshRenderer != null)
            playerRefs.bodyMeshRenderer.enabled = false;

        InsideCloset = true;

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
            playerRefs.rb.linearVelocity = Vector3.zero;
            playerRefs.rb.angularVelocity = Vector3.zero;
            playerRefs.rb.isKinematic = true;
        }

        player.position = exitPoint.position;
        player.rotation = exitPoint.rotation;

        playerRefs.playerCam.Priority = 100;
        closetCam.Priority = 10;

        InsideCloset = false;

        if (stalkerFollowTarget != null)
        {
            stalkerFollowTarget.SetActive(false);
        }

        if (closetAnim != null)
            closetAnim.SetInteger("C", 1);

        yield return new WaitForSeconds(1f);

        if (closetAnim != null)
            closetAnim.SetInteger("C", 0);

        if (playerRefs.movementScript != null)
            playerRefs.movementScript.enabled = true;

        if (playerRefs.flashlightScript != null)
            playerRefs.flashlightScript.enabled = true;

        if (playerRefs.bodyMeshRenderer != null)
            playerRefs.bodyMeshRenderer.enabled = true;

        if (playerRefs.rb != null)
        {
            playerRefs.rb.isKinematic = false;
            playerRefs.rb.linearVelocity = Vector3.zero;
            playerRefs.rb.angularVelocity = Vector3.zero;
        }

        isTransitioning = false;
    }
}