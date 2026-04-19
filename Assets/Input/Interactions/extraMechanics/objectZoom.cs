using UnityEngine;
using Unity.Cinemachine;

public class objectZoom : MonoBehaviour
{
    [Header("Interactable Object")]
    [SerializeField] private MonoBehaviour interactableObject;
    private IZoomInteractable mainObjHandler;

    [Header("Cinemachine Cameras")]
    public CinemachineCamera playerVCam;
    public CinemachineCamera puzzleVCam;

    [Header("Player Settings")]
    [SerializeField] public PlayerMovement playerController;
    [SerializeField] public PlayerLook playerlookCamera;
    public Flashlight fl;

    public bool isInPuzzle = false;
    private bool canInteract = true;

    private Rigidbody playerRb;

    void Start()
    {
        if (interactableObject != null)
            mainObjHandler = interactableObject as IZoomInteractable;

        if (mainObjHandler == null)
            mainObjHandler = GetComponent<IZoomInteractable>();

        // Default camera priorities
        playerVCam.Priority.Value = 20;
        puzzleVCam.Priority.Value = -10;

        if (playerController != null)
            playerRb = playerController.GetComponent<Rigidbody>();
    }

    public void InteractZoomObj()
    {
        if (!canInteract) return;

        canInteract = false;
        isInPuzzle = !isInPuzzle;

        if (mainObjHandler != null)
            mainObjHandler.IsInteracting = isInPuzzle;

        if (isInPuzzle)
            EnterPuzzle();
        else
            ExitPuzzle();

        Invoke(nameof(ResetInteract), 0.2f);
    }

    private void EnterPuzzle()
    {
        // Switch camera
        puzzleVCam.Priority.Value = 20;
        playerVCam.Priority.Value = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player look
        if (playerlookCamera != null)
            playerlookCamera.enabled = false;

        // Freeze player and disable movement
        if (playerController != null)
        {
            StopPlayerInstantly();
            playerController.enabled = false;
        }

        if (fl != null){
            fl.torchLight.enabled = false;
            fl.enabled = false;
            
        }


        mainObjHandler?.StartInteraction();
    }

    public void ExitPuzzle()
    {
        // Switch camera back
        playerVCam.Priority.Value = 20;
        puzzleVCam.Priority.Value = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player look
        if (playerlookCamera != null)
            playerlookCamera.enabled = true;

        // Unfreeze player and re-enable movement
        if (playerController != null)
            playerController.enabled = true;

        if (playerRb != null)
            playerRb.isKinematic = false;

        if (fl != null)
            fl.enabled = true;
        

        mainObjHandler?.StopInteraction();
    }

    private void StopPlayerInstantly()
    {
        if (playerController == null) return;

        // Freeze Rigidbody to prevent movement/jumping
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;
        }
    }

    private void ResetInteract()
    {
        canInteract = true;
    }
}