using UnityEngine;
using Unity.Cinemachine;

public class objectZoom : MonoBehaviour
{
    public GameObject interactableText;

    [SerializeField] private MonoBehaviour interactableObject;
    private IZoomInteractable mainObjHandler;

    public CinemachineCamera playerVCam;
    public CinemachineCamera puzzleVCam;

    public PlayerMovement playerController;
    public PlayerLook playerlookCamera;
    public Flashlight fl;

    public bool isInPuzzle = false;
    private bool canInteract = true;

    private Rigidbody playerRb;

    void Start()
    {
        EnsureHandler();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerMovement>();

        if (playerlookCamera == null)
            playerlookCamera = FindFirstObjectByType<PlayerLook>();

        if (fl == null)
            fl = FindFirstObjectByType<Flashlight>();

        if (playerController != null)
            playerRb = playerController.GetComponent<Rigidbody>();

        if (interactableText == null)
        {
            GameObject ui = GameObject.FindWithTag("InteractText");
            if (ui != null)
                interactableText = ui;
        }

        if (playerVCam == null)
            playerVCam = FindCamera("vcam");

        if (puzzleVCam == null)
            puzzleVCam = FindCamera("puzzlecam");

        if (playerVCam != null)
            playerVCam.Priority = 100;

        if (puzzleVCam != null)
            puzzleVCam.Priority = 0;
    }

    void Update()
    {
        if (mainObjHandler == null)
            EnsureHandler();
    }

    public void InteractZoomObj()
    {
        if (!canInteract) return;

        EnsureHandler();

        canInteract = false;
        isInPuzzle = !isInPuzzle;

        if (mainObjHandler != null)
            mainObjHandler.IsInteracting = isInPuzzle;

        if (isInPuzzle)
            EnterPuzzle();
        else
            ExitPuzzle();

        Invoke(nameof(ResetInteract), 0.5f);
    }

    private void EnterPuzzle()
    {
        if (interactableText != null)
            interactableText.SetActive(false);

        if (playerVCam != null)
            playerVCam.Priority = 0;

        if (puzzleVCam != null)
            puzzleVCam.Priority = 200;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerlookCamera != null)
            playerlookCamera.enabled = false;

        if (playerController != null)
        {
            StopPlayerInstantly();
            playerController.enabled = false;
        }

        if (fl != null)
            fl.enabled = false;

        mainObjHandler?.StartInteraction();
    }

    public void ExitPuzzle()
    {
        if (interactableText != null)
            interactableText.SetActive(true);

        if (playerVCam != null)
            playerVCam.Priority = 100;

        if (puzzleVCam != null)
            puzzleVCam.Priority = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerlookCamera != null)
            playerlookCamera.enabled = true;

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
        if (playerRb == null) return;

        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
        playerRb.isKinematic = true;
    }

    private void ResetInteract()
    {
        canInteract = true;
    }

    private void EnsureHandler()
    {
        if (mainObjHandler == null)
        {
            mainObjHandler = GetComponent<IZoomInteractable>();
            if (mainObjHandler == null)
                mainObjHandler = GetComponentInParent<IZoomInteractable>();
        }
    }

    private CinemachineCamera FindCamera(string keyword)
    {
        CinemachineCamera[] cams =
            FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cams)
        {
            if (cam.gameObject.name.ToLower().Contains(keyword.ToLower()))
                return cam;
        }

        return null;
    }
}