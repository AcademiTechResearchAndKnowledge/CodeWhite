using UnityEngine;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Dependencies")]
    [Tooltip("The disabled GameObject holding the Timeline (PlayableDirector).")]
    public GameObject timelineGameObject;

    [Header("Dynamic Search Settings")]
    [Tooltip("The Tag applied to your main UI Parent/Canvas so the script can find it across scenes.")]
    public string uiTag = "MainUI";

    [Tooltip("Check this if the cutscene should play immediately when the player spawns into this scene.")]
    public bool playOnSpawn = false;

    [HideInInspector]
    public GameObject uiParentGameObject;

    [Tooltip("Reference to the player to disable controls. (Will auto-find if left empty)")]
    public PlayerReferences playerRefs;

    private bool hasPlayed = false;

    private void Start()
    {
        FindSceneDependencies();

        // Triggers automatically if you want it to play right when you enter the level
        if (playOnSpawn)
        {
            ActivateCutscene();
        }
    }

    private void FindSceneDependencies()
    {
        if (playerRefs == null)
        {
            playerRefs = Object.FindFirstObjectByType<PlayerReferences>();
            if (playerRefs == null)
            {
                Debug.LogWarning("CutsceneManager couldn't find a PlayerReferences component in this scene!");
            }
        }

        if (uiParentGameObject == null)
        {
            uiParentGameObject = GameObject.FindWithTag(uiTag);
            if (uiParentGameObject == null)
            {
                Debug.LogWarning($"CutsceneManager couldn't find a UI GameObject with the tag '{uiTag}' in this scene!");
            }
        }
    }

    public void ActivateCutscene()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        FindSceneDependencies();
        DisablePlayerControls();

        if (uiParentGameObject != null)
        {
            uiParentGameObject.SetActive(false);
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

        if (uiParentGameObject != null)
        {
            uiParentGameObject.SetActive(true);
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
    private void OnEnable()
    {
        DeathMenu.OnPlayerRestart += ResetCutsceneState;
    }

    private void OnDisable()
    {
        DeathMenu.OnPlayerRestart -= ResetCutsceneState;
    }

    private void ResetCutsceneState()
    {
        hasPlayed = false;
        DeactivateCutscene();
    }
}