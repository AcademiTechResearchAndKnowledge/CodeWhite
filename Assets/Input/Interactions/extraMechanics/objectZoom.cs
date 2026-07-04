using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

    [SerializeField] private Outline outline;

    public bool isInPuzzle = false;

    private Rigidbody playerRb;

    private float lastInteractTime;
    private float puzzleEnterTime;

    public float interactCooldown = 0.2f;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        StartCoroutine(StartRoutine());
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator StartRoutine()
    {
        EnsureHandler();
        playerVCam = PlayerCameraReference.Instance;
        yield return null;
        StartCoroutine(BindAllRoutine());
        yield return null;
        isInPuzzle = false;
        SetCameraState(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isInPuzzle = false;
        StartCoroutine(BindAllRoutine());
        SetCameraState(false);
    }

    private IEnumerator BindAllRoutine()
    {
        yield return null;
        BindPlayer();
        yield return null;
        EnsureHandler();
    }

    private void BindPlayer()
    {
        playerController = FindFirstObjectByType<PlayerMovement>();
        playerlookCamera = FindFirstObjectByType<PlayerLook>();
        fl = FindFirstObjectByType<Flashlight>();
        if (playerController != null)
            playerRb = playerController.GetComponent<Rigidbody>();
        else
            playerRb = null;
    }

    void Update()
    {   
        if (isInPuzzle && outline != null)
            outline.enabled = false;
            
        if (PauseMenu.GameIsPaused)
            return;

        if (mainObjHandler == null)
            EnsureHandler();

        if (interactableText == null && !isInPuzzle)
            interactableText = GameObject.FindWithTag("InteractText");

        if (interactableText != null && isInPuzzle)
            interactableText.SetActive(false);

        if (isInPuzzle &&
            Time.time - puzzleEnterTime > 0.2f &&
            Keyboard.current.fKey.wasPressedThisFrame)
        {
            ExitPuzzle();

            if (interactableText != null)
                interactableText.SetActive(true);

            LaptopManager.Instance?.StopInteraction(true);

            if (mainObjHandler != null)
                mainObjHandler.IsInteracting = false;

            SetCameraState(false);
        }
    }

    public void InteractZoomObj()
    {
        if (PauseMenu.GameIsPaused)
            return;

        if (Time.time - lastInteractTime < interactCooldown)
            return;

        lastInteractTime = Time.time;

        EnsureHandler();

        isInPuzzle = !isInPuzzle;

        if (interactableText != null)
            interactableText.SetActive(!isInPuzzle);

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
    }

    private void EnterPuzzle()
    {
        if (PauseMenu.GameIsPaused)
            return;

        puzzleEnterTime = Time.time;

        if (outline != null)
            outline.enabled = false;

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
        if (PauseMenu.GameIsPaused)
            return;

        if (outline != null)
            outline.enabled = true;

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

        SetCameraState(false);

        isInPuzzle = false;
    }

    private void StopPlayerInstantly()
    {
        if (playerRb == null)
            return;

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

        if (interactableText == null)
            interactableText = GameObject.FindWithTag("InteractText");
    }
}