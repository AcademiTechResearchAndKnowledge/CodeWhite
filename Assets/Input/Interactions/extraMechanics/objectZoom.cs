using UnityEngine;

public class objectZoom : MonoBehaviour
{
    [Header("Interactable Object")]
    [SerializeField] private MonoBehaviour interactableObject;
    private IZoomInteractable mainObjHandler;

    [Header("Camera Settings")]
    public Camera playerCamera;
    public Transform cameraFocus;
    [Range(1f, 20f)] public float moveSpeed = 5f;
    [Range(1f, 360f)] public float rotateSpeed = 180f;

    [Header("Player Settings")]
    [SerializeField] public PlayerMovement playerController;
    [SerializeField] public PlayerLook playerlookCamera;

    public bool isInPuzzle = false;
    private bool canInteract = true;

    private Vector3 savedLocalPos;
    private Quaternion savedLocalRot;

    private bool returningCamera = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Convert MonoBehaviour to interface
        if (interactableObject != null)
            mainObjHandler = interactableObject as IZoomInteractable;

        // Optional auto-detect if not assigned
        if (mainObjHandler == null)
            mainObjHandler = GetComponent<IZoomInteractable>();
    }

    void LateUpdate()
    {
        if (playerCamera == null || cameraFocus == null) return;

        if (isInPuzzle)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            playerCamera.transform.position = Vector3.MoveTowards(
                playerCamera.transform.position,
                cameraFocus.position,
                moveSpeed * Time.deltaTime
            );

            Vector3 targetEuler = cameraFocus.rotation.eulerAngles;
            targetEuler.z = 0f;

            Quaternion targetRot = Quaternion.Euler(targetEuler);

            playerCamera.transform.rotation = Quaternion.RotateTowards(
                playerCamera.transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
        else if (returningCamera)
        {
            Cursor.visible = false;
            playerCamera.transform.localPosition = Vector3.MoveTowards(
                playerCamera.transform.localPosition,
                savedLocalPos,
                moveSpeed * Time.deltaTime
            );

            playerCamera.transform.localRotation = Quaternion.RotateTowards(
                playerCamera.transform.localRotation,
                savedLocalRot,
                rotateSpeed * Time.deltaTime
            );

            if (Vector3.Distance(playerCamera.transform.localPosition, savedLocalPos) < 0.01f &&
                Quaternion.Angle(playerCamera.transform.localRotation, savedLocalRot) < 0.5f)
            {
                playerCamera.transform.localPosition = savedLocalPos;
                playerCamera.transform.localRotation = savedLocalRot;

                returningCamera = false;

                if (playerlookCamera != null)
                    playerlookCamera.enabled = true;
            }
        }

        // Absolute roll lock safeguard
        Vector3 rot = playerCamera.transform.eulerAngles;
        playerCamera.transform.rotation = Quaternion.Euler(rot.x, rot.y, 0f);
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
        if (playerCamera != null)
        {
            savedLocalPos = playerCamera.transform.localPosition;
            savedLocalRot = playerCamera.transform.localRotation;
        }

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
        returningCamera = true;

        if (playerController != null)
            playerController.enabled = true;

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