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

    private void Start()
    {
        CloseInspection();
    }

    private void Update()
    {
        if (!isInspecting) return;

        // Safeguard: Don't allow closing on the exact same frame it opened
        if (openedThisFrame)
        {
            openedThisFrame = false;
            return;
        }

        // Check for E or ESC using the New Input System
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
        openedThisFrame = true; // Prevents instant closing
        mainPanel.SetActive(true);

        // Reset visuals
        signedVisual.SetActive(false);
        forgedVisual.SetActive(false);
        unsignedVisual.SetActive(false);

        switch (bookData.bookType)
        {
            case LibraryBookType.Signed: signedVisual.SetActive(true); break;
            case LibraryBookType.Forged: forgedVisual.SetActive(true); break;
            case LibraryBookType.Unsigned: unsignedVisual.SetActive(true); break;
        }

        // Optional: Block player movement here
    }

    public void CloseInspection()
    {
        isInspecting = false;
        mainPanel.SetActive(false);
        // Optional: Re-enable player movement here
    }

    // Helper method for the Inventory Manager to check status
    public bool IsOpen() => isInspecting;
}