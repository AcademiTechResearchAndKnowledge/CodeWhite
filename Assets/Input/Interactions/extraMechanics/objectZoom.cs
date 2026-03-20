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

    public bool isInPuzzle = false;
    private bool canInteract = true;

    void Start()
    {
        
        if (interactableObject != null)
            mainObjHandler = interactableObject as IZoomInteractable;

        if (mainObjHandler == null)
            mainObjHandler = GetComponent<IZoomInteractable>();

        // Default state
        playerVCam.Priority.Value = 20;
        puzzleVCam.Priority.Value = -10;
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

        if (playerlookCamera != null)
            playerlookCamera.enabled = false;

        if (playerController != null)
        {
            StopPlayerInstantly();
            playerController.enabled = false;
        }

        mainObjHandler?.StartInteraction();
    }

    private void ExitPuzzle()
    {
        // Switch back
        playerVCam.Priority.Value = 20;
        puzzleVCam.Priority.Value = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.enabled = true;

        if (playerlookCamera != null)
            playerlookCamera.enabled = true;

        mainObjHandler?.StopInteraction();
    }

    private void StopPlayerInstantly()
    {
        if (playerController == null) return;

        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        CharacterController cc = playerController.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.Move(Vector3.zero);
        }
    }

    private void ResetInteract()
    {
        canInteract = true;
    }
}