using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using System.Collections;

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

    private Rigidbody playerRb;

    private float lastInteractTime;
    public float interactCooldown = 0.2f;

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

        if (playerVCam == null)
            Debug.LogError("Player VCam NOT FOUND");

        if (puzzleVCam == null)
            Debug.LogError("Puzzle VCam NOT FOUND");
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

        StartCoroutine(SwitchCamera());
    }

    private IEnumerator SwitchCamera()
    {
        yield return null;

        if (playerVCam != null)
            playerVCam.Priority = isInPuzzle ? 0 : 100;

        if (puzzleVCam != null)
            puzzleVCam.Priority = isInPuzzle ? 200 : 0;

        Debug.Log("CAM SWITCH | playerCam: " + playerVCam.Priority + " puzzleCam: " + puzzleVCam.Priority);
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