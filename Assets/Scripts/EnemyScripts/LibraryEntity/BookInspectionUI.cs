using UnityEngine;
using UnityEngine.InputSystem;

public class BookInspectionUI : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainPanel;

    [Header("Signature Visuals")]
    public GameObject signedVisual;
    public GameObject forgedVisual;
    public GameObject unsignedVisual;

    private bool isInspecting = false;
    private bool openedThisFrame = false;

    private PlayerReferences playerRefs;

    private void Start()
    {
        playerRefs = FindFirstObjectByType<PlayerReferences>();
        CloseInspection();
    }

    private void Update()
    {
        if (!isInspecting) return;
        if (PauseMenu.GameIsPaused) return; // Stop processing book inputs if paused

        if (openedThisFrame)
        {
            openedThisFrame = false;
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                CloseInspection();
            }
        }
    }

    public void OpenInspection(ObjectiveItemData bookData)
    {
        isInspecting = true;
        openedThisFrame = true;
        mainPanel.SetActive(true);

        signedVisual.SetActive(false);
        forgedVisual.SetActive(false);
        unsignedVisual.SetActive(false);

        switch (bookData.bookType)
        {
            case LibraryBookType.Signed: signedVisual.SetActive(true); break;
            case LibraryBookType.Forged: forgedVisual.SetActive(true); break;
            case LibraryBookType.Unsigned: unsignedVisual.SetActive(true); break;
        }

        if (playerRefs != null && playerRefs.playerLook != null)
        {
            playerRefs.playerLook.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInspection()
    {
        isInspecting = false;
        mainPanel.SetActive(false);

        if (playerRefs != null && playerRefs.playerLook != null)
        {
            playerRefs.playerLook.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- NEW FIXED FUNCTION FOR PAUSE OVERRIDE ---
    public void CloseInspectionFromPause()
    {
        isInspecting = false;
        mainPanel.SetActive(false);
        // We DO NOT change the cursor or player look here.
        // We let the Pause Menu script handle it cleanly.
    }

    public bool IsOpen() => isInspecting;
}