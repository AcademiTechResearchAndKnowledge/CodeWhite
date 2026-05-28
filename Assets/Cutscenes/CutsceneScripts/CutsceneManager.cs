using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Dependencies")]
    [Tooltip("The disabled GameObject holding the Timeline (PlayableDirector).")]
    public GameObject timelineGameObject;

    [Tooltip("Reference to the player to disable controls. (Will auto-find if left empty)")]
    public PlayerReferences playerRefs;

    private bool hasPlayed = false;

    private void Start()
    {
        FindPlayerReferences();
    }

    private void FindPlayerReferences()
    {
        if (playerRefs != null) return;

        playerRefs = Object.FindFirstObjectByType<PlayerReferences>();

        if (playerRefs == null)
        {
            Debug.LogWarning("CutsceneManager couldn't find a PlayerReferences component in the scene!");
        }
    }

    public void ActivateCutscene()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        FindPlayerReferences();

        DisablePlayerControls();

        if (PersistentUI.Instance != null)
        {
            PersistentUI.Instance.SetUIVisibility(false);
        }

        if (timelineGameObject != null)
        {
            timelineGameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Timeline GameObject is not assigned in the CutsceneManager!");
        }
    }

    public void DeactivateCutscene()
    {
        if (timelineGameObject != null)
        {
            timelineGameObject.SetActive(false);
        }

        if (PersistentUI.Instance != null)
        {
            PersistentUI.Instance.SetUIVisibility(true);
        }

        EnablePlayerControls();
    }

    private void DisablePlayerControls()
    {
        if (playerRefs != null)
        {
            if (playerRefs.movementScript != null) playerRefs.movementScript.enabled = false;
            if (playerRefs.playerLook != null) playerRefs.playerLook.enabled = false;
            if (playerRefs.flashlightScript != null) playerRefs.flashlightScript.enabled = false;

            if (playerRefs.rb != null)
            {
                playerRefs.rb.linearVelocity = Vector3.zero;
                playerRefs.rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void EnablePlayerControls()
    {
        if (playerRefs != null)
        {
            if (playerRefs.movementScript != null) playerRefs.movementScript.enabled = true;
            if (playerRefs.playerLook != null) playerRefs.playerLook.enabled = true;
            if (playerRefs.flashlightScript != null) playerRefs.flashlightScript.enabled = true;
        }
    }
}