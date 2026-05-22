using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Dependencies")]
    [Tooltip("The disabled GameObject holding the Timeline (PlayableDirector).")]
    public GameObject timelineGameObject;

    [Tooltip("Reference to the player to disable controls.")]
    public PlayerReferences playerRefs;

    // Tracks if the cutscene has already been triggered
    private bool hasPlayed = false;

    public void ActivateCutscene()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        DisablePlayerControls();

        // Hide the Persistent UI globally
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

        // Show the Persistent UI again when the cutscene finishes
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