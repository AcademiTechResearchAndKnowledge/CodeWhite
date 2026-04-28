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

    [Header("Cameras")]
    public CinemachineCamera playerVCam;
    public CinemachineCamera puzzleVCam;

    [Header("Player")]
    public PlayerMovement playerController;
    public PlayerLook playerlookCamera;
    public Flashlight fl;

    public bool isInPuzzle = false;

    private Rigidbody playerRb;

    private float lastInteractTime;
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

        if (playerVCam == null)
            Debug.LogError("Player VCam NOT FOUND (PlayerCameraReference missing)");

        if (puzzleVCam == null)
            Debug.LogError("Puzzle VCam NOT ASSIGNED in Inspector");

        yield return null;

        StartCoroutine(BindAllRoutine());

        SetCameraState(false);

        Debug.Log("INITIAL CAMERA STATE APPLIED");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("SCENE LOADED | Rebinding objectZoom references...");
        StartCoroutine(BindAllRoutine());
    }

    private IEnumerator BindAllRoutine()
    {
        yield return null;

        BindPlayer();
        yield return null;
        BindUI();
        EnsureHandler();

        Debug.Log("objectZoom BIND COMPLETE");
    }

    private void BindPlayer()
    {
        playerController = FindFirstObjectByType<PlayerMovement>();
        playerlookCamera = FindFirstObjectByType<PlayerLook>();
        fl = FindFirstObjectByType<Flashlight>();

        if (playerController != null)
        {
            playerRb = playerController.GetComponent<Rigidbody>();
            Debug.Log("PLAYER LINKED OK");
        }
        else
        {
            playerRb = null;
            Debug.LogWarning("PLAYER NOT FOUND IN THIS SCENE");
        }
    }

    private void BindUI()
    {
        StartCoroutine(WaitForUI());
    }

    private IEnumerator WaitForUI()
    {
        int attempts = 0;

        while (interactableText == null && attempts < 50)
        {
            GameObject ui = GameObject.FindWithTag("InteractText");

            if (ui != null)
            {
                interactableText = ui;
                Debug.Log("INTERACTABLE TEXT LINKED");
                yield break;
            }

            attempts++;
            yield return new WaitForSeconds(0.1f);
        }

        if (interactableText == null)
            Debug.LogWarning("INTERACTABLE TEXT NOT FOUND IN SCENE");
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

        SetCameraState(false);   
        isInPuzzle = false;

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