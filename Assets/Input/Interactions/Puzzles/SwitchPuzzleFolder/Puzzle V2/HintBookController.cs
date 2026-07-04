using UnityEngine;
using UnityEngine.InputSystem;

public class HintBookController : MonoBehaviour
{
    [Header("UI Setup")]
    [Tooltip("Drag the parent Panel or Canvas GameObject of the Hint Book here.")]
    public GameObject hintCanvasPanel;

    private PlayerReferences playerRefs;
    private bool isUiOpen = false;
    private bool skipFrame = false;

    void Start()
    {
        if (hintCanvasPanel != null)
        {
            hintCanvasPanel.SetActive(false);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerRefs = playerObj.GetComponent<PlayerReferences>();
        }
        else
        {
            Debug.LogWarning("[HintBookController] Could not find GameObject with tag 'Player'!");
        }
    }

    void Update()
    {
        if (!isUiOpen) return;
        if (PauseMenu.GameIsPaused) return;

        if (skipFrame)
        {
            skipFrame = false;
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame)
            {
                CloseHintBook();
            }
        }
    }

    public void OpenHintBook()
    {
        if (hintCanvasPanel == null) return;

        hintCanvasPanel.SetActive(true);
        isUiOpen = true;
        skipFrame = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerRefs != null)
        {
            if (playerRefs.interactionScript != null) playerRefs.interactionScript.enabled = false;
            if (playerRefs.movementScript != null) playerRefs.movementScript.enabled = false;
            if (playerRefs.playerLook != null) playerRefs.playerLook.enabled = false;
            if (playerRefs.flashlightScript != null) playerRefs.flashlightScript.enabled = false;

            if (playerRefs.rb != null)
            {
                playerRefs.rb.linearVelocity = Vector3.zero;
            }
        }
    }

    public void CloseHintBook()
    {
        if (hintCanvasPanel == null) return;

        hintCanvasPanel.SetActive(false);
        isUiOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerRefs != null)
        {
            if (playerRefs.interactionScript != null) playerRefs.interactionScript.enabled = true;
            if (playerRefs.movementScript != null) playerRefs.movementScript.enabled = true;
            if (playerRefs.playerLook != null) playerRefs.playerLook.enabled = true;
            if (playerRefs.flashlightScript != null) playerRefs.flashlightScript.enabled = true;
        }
    }

    public void CloseHintBookFromPause()
    {
        if (hintCanvasPanel == null) return;

        hintCanvasPanel.SetActive(false);
        isUiOpen = false;
    }

    public bool IsOpen() => isUiOpen;
}