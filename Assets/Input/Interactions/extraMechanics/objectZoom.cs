using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class objectZoom : MonoBehaviour
{
    public GameObject interactableText;

    [SerializeField] private MonoBehaviour interactableObject;
    private IZoomInteractable mainObjHandler;

    [Header("Cameras")]
    public CinemachineCamera playerVCam;   // runtime assigned
    public CinemachineCamera puzzleVCam;   // assign in Inspector

    [Header("Player")]
    public PlayerMovement playerController;
    public PlayerLook playerlookCamera;
    public Flashlight fl;

    public bool isInPuzzle = false;

    private Rigidbody playerRb;

    private float lastInteractTime;
    public float interactCooldown = 0.2f;

    void Start()
    {
        EnsureHandler();

    
        playerVCam = PlayerCameraReference.Instance;

        if (playerVCam == null)
            Debug.LogError("Player VCam NOT FOUND (PlayerCameraReference missing)");

        if (puzzleVCam == null)
            Debug.LogError("Puzzle VCam NOT ASSIGNED in Inspector");

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


        SetCameraState(false);
    }

    void Update()
    {
        if (mainObjHandler == null)
            EnsureHandler();

        if (isInPuzzle && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ExitPuzzle();
            isInPuzzle = false;

            if (mainObjHandler != null)
                mainObjHandler.IsInteracting = false;

            SetCameraState(false);
        }
    }

    public void InteractZoomObj()
    {
        if (Time.time - lastInteractTime < interactCooldown) return;
        lastInteractTime = Time.time;

        EnsureHandler();

        isInPuzzle = !isInPuzzle;

        if (mainObjHandler != null)
            mainObjHandler.IsInteracting = isInPuzzle;

        if (isInPuzzle)
            EnterPuzzle();
        else
            ExitPuzzle();

        SetCameraState(isInPuzzle);
    }

    
    private void SetCameraState(bool puzzleActive)
    {
        if (playerVCam != null)
            playerVCam.Priority = puzzleActive ? 0 : 100;

        if (puzzleVCam != null)
            puzzleVCam.Priority = puzzleActive ? 200 : 0;

        Debug.Log($"CAM SWITCH | player: {playerVCam?.Priority} puzzle: {puzzleVCam?.Priority}");
    }

    private void EnterPuzzle()
    {
        if (interactableText != null)
            interactableText.SetActive(false);

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

        Debug.Log("ENTER PUZZLE");
    }

    public void ExitPuzzle()
    {
        if (interactableText != null)
            interactableText.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerlookCamera != null)
            playerlookCamera.enabled = true;

        if (playerController != null)
            playerController.enabled = true;

        if (fl != null)
            fl.enabled = true;

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }

        mainObjHandler?.StopInteraction();

        Debug.Log("EXIT PUZZLE");
    }

    private void StopPlayerInstantly()
    {
        if (playerRb == null) return;

        playerRb.linearVelocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
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
}