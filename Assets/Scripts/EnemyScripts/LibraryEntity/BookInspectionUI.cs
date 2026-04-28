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

    // --- NEW: Cache the player references ---
    private PlayerReferences playerRefs;

    private void Start()
    {
        // Find the player references once when the game starts
        playerRefs = FindFirstObjectByType<PlayerReferences>();

        CloseInspection();
    }

    private void Update()
    {
        if (!isInspecting) return;

        if (openedThisFrame)
        {
            openedThisFrame = false;
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
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

        // --- NEW: Disable Mouse Look ---
        if (playerRefs != null && playerRefs.playerLook != null)
        {
            playerRefs.playerLook.enabled = false;
        }

        // Unlock cursor so the player can use the UI "Close" button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseInspection()
    {
        isInspecting = false;
        mainPanel.SetActive(false);

        // --- NEW: Re-enable Mouse Look ---
        if (playerRefs != null && playerRefs.playerLook != null)
        {
            playerRefs.playerLook.enabled = true;
        }

        // Lock cursor back to the center of the screen for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool IsOpen() => isInspecting;
}