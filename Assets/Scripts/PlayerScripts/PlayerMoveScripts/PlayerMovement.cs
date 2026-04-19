using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Transform body;
    [SerializeField] private Transform cameraHolder;

    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    [SerializeField] private float sprintMultiplier = 1.3f;
    [SerializeField] private float exhaustedMultiplier = 0.5f;

    [Header("Ideal Crouch Settings")]
    [Tooltip("The total height of the player when crouching.")]
    [SerializeField] private float crouchHeight = 0.5f;

    [Tooltip("Must be half of crouchHeight or less!")]
    [SerializeField] private float crouchCapsuleRadius = 0.25f;

    [Tooltip("Multiplier for movement speed while crouching (e.g., 0.5 is half speed).")]
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Tooltip("How fast the player transitions between standing and crouching.")]
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Environment Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public LayerMask ceilingMask;

    [Header("Colliders")]
    [SerializeField] private SphereCollider sphereCol;

    [Header("Sphere Collider Settings")]
    [SerializeField] private float crouchSphereRadius = 0.05f;

    private float originalSphereRadius;
    private Vector3 originalSphereCenter;

    private Rigidbody rb;
    private CapsuleCollider col;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;
    public bool isCrouching;

    private float normalHeight;
    private float normalCapsuleRadius;
    private Vector3 normalCenter;

    private Vector3 originalBodyPos;
    private Vector3 originalCameraPos;

    private float currentCrouchWeight = 0f;

    private PlayerStats playerStats;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        playerStats = GetComponent<PlayerStats>();

        if (col != null)
        {
            normalHeight = col.height;
            normalCapsuleRadius = col.radius;
            normalCenter = col.center;
        }

        if (sphereCol != null)
        {
            originalSphereRadius = sphereCol.radius;
            originalSphereCenter = sphereCol.center;
        }
        else
        {
            Debug.LogWarning("Sphere Collider is not assigned in the PlayerMovement script!");
        }

        if (body != null)
            originalBodyPos = body.localPosition;

        if (cameraHolder != null)
            originalCameraPos = cameraHolder.localPosition;
    }

    void Update()
    {
        CheckGround();
        isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        HandleCrouch();
        HandleStamina();


        Vector3 origin = new Vector3(transform.position.x, col.bounds.max.y + 0.01f, transform.position.z);
        float distance = normalHeight + 0.1f;
        Debug.DrawRay(origin, Vector3.up * distance, Color.red);
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void OnJump()
    {
        if (!isGrounded) return;

        if (isCrouching && !CanStand())
            return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void HandleCrouch()
    {
        if (Keyboard.current == null) return;

        bool wantsToCrouch = Keyboard.current.leftCtrlKey.isPressed;

        if (!wantsToCrouch && currentCrouchWeight > 0f && !CanStand())
        {
            wantsToCrouch = true;
        }

        isCrouching = wantsToCrouch;

        float targetWeight = wantsToCrouch ? 1f : 0f;

        currentCrouchWeight = Mathf.Lerp(currentCrouchWeight, targetWeight, Time.deltaTime * crouchTransitionSpeed);

        float heightDifference = normalHeight - crouchHeight;
        float currentDrop = (heightDifference / 2f) * currentCrouchWeight;

        if (col != null)
        {
            col.height = Mathf.Lerp(normalHeight, crouchHeight, currentCrouchWeight);
            float targetRadius = Mathf.Min(crouchCapsuleRadius, crouchHeight / 2f);
            col.radius = Mathf.Lerp(normalCapsuleRadius, targetRadius, currentCrouchWeight);
            col.center = new Vector3(normalCenter.x, normalCenter.y - currentDrop, normalCenter.z);
        }

        if (sphereCol != null)
        {
            sphereCol.radius = Mathf.Lerp(originalSphereRadius, crouchSphereRadius, currentCrouchWeight);
            Vector3 targetSphereCenter = originalSphereCenter + new Vector3(0, -0.3f, 0);
            sphereCol.center = Vector3.Lerp(originalSphereCenter, targetSphereCenter, currentCrouchWeight);
        }

        if (body != null)
            body.localPosition = originalBodyPos + new Vector3(0, -currentDrop, 0);

        if (cameraHolder != null)
            cameraHolder.localPosition = originalCameraPos + new Vector3(0, -currentDrop, 0);
    }

    bool CanStand()
    {
        if (col == null) return true;

        Vector3 centerWorld = transform.TransformPoint(normalCenter);
        float offset = (normalHeight / 2f) - normalCapsuleRadius;

        Vector3 pointBottom = centerWorld - transform.up * offset;
        Vector3 pointTop = centerWorld + transform.up * offset;

        float checkRadius = normalCapsuleRadius * 0.95f;

        return !Physics.CheckCapsule(pointBottom, pointTop, checkRadius, ceilingMask, QueryTriggerInteraction.Ignore);
    }

    void HandleStamina()
    {
        if (playerStats == null) return;

        bool isActuallyMoving = HorizontalSpeed > 0.1f;

        if (isSprinting && isActuallyMoving && playerStats.HasStamina() && !isCrouching)
        {
            playerStats.DrainStamina(playerStats.GetDrainRate() * Time.deltaTime);
        }
        else if (!isSprinting)
        {
            playerStats.RegenerateStamina(playerStats.GetRegenRate() * Time.deltaTime);
        }
    }

    void MovePlayer()
    {
        Vector3 direction = body.right * moveInput.x + body.forward * moveInput.y;
        direction.Normalize();

        float currentSpeed = moveSpeed;
        bool isMoving = moveInput.magnitude > 0.1f;

        if (playerStats != null)
        {
            if (playerStats.Stamina <= 0f)
                currentSpeed = moveSpeed * exhaustedMultiplier;
            else if (isSprinting && isMoving && !isCrouching)
                currentSpeed = moveSpeed * sprintMultiplier;
        }

        if (isCrouching)
            currentSpeed *= crouchSpeedMultiplier;

        rb.linearVelocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);
    }

    public bool IsGrounded => isGrounded;

    public float HorizontalSpeed
    {
        get
        {
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            return v.magnitude;
        }
    }
}