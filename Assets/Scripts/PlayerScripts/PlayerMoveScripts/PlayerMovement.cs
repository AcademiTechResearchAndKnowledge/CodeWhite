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

    [Header("Ideal Standing Settings")]
    [Tooltip("Right-click this component and select 'Fetch Standing Values' to auto-fill these!")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float standCapsuleRadius = 0.5f;
    [SerializeField] private Vector3 standCenter = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 standCameraPos = new Vector3(0, 0.8f, 0);
    [SerializeField] private Vector3 standBodyPos = Vector3.zero;

    [Header("Ideal Crouch Settings")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float crouchCapsuleRadius = 0.25f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Environment Detection")]
    public Transform groundCheck;
    public float groundDistance = 0.45f;
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

    private float currentCrouchWeight = 0f;
    private PlayerStats playerStats;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        playerStats = GetComponent<PlayerStats>();

        if (sphereCol != null)
        {
            originalSphereRadius = sphereCol.radius;
            originalSphereCenter = sphereCol.center;
        }
    }

    void Update()
    {
        CheckGround();
        isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        HandleCrouch();
        HandleStamina();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void OnJump()
    {
        if (!isGrounded) return;
        if (isCrouching && !CanStand()) return;

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

        float heightDifference = standHeight - crouchHeight;
        float currentDrop = (heightDifference / 2f) * currentCrouchWeight;

        if (col != null)
        {
            col.height = Mathf.Lerp(standHeight, crouchHeight, currentCrouchWeight);
            float targetRadius = Mathf.Min(crouchCapsuleRadius, crouchHeight / 2f);
            col.radius = Mathf.Lerp(standCapsuleRadius, targetRadius, currentCrouchWeight);
            col.center = new Vector3(standCenter.x, standCenter.y - currentDrop, standCenter.z);
        }

        if (sphereCol != null)
        {
            sphereCol.radius = Mathf.Lerp(originalSphereRadius, crouchSphereRadius, currentCrouchWeight);
            Vector3 targetSphereCenter = originalSphereCenter + new Vector3(0, -0.3f, 0);
            sphereCol.center = Vector3.Lerp(originalSphereCenter, targetSphereCenter, currentCrouchWeight);
        }

        if (body != null)
            body.localPosition = standBodyPos + new Vector3(0, -currentDrop, 0);

        if (cameraHolder != null)
            cameraHolder.localPosition = standCameraPos + new Vector3(0, -currentDrop, 0);
    }

    bool CanStand()
    {
        if (col == null) return true;

        Vector3 centerWorld = transform.TransformPoint(standCenter);
        float offset = (standHeight / 2f) - standCapsuleRadius;

        Vector3 pointBottom = centerWorld - transform.up * offset;
        Vector3 pointTop = centerWorld + transform.up * offset;

        float checkRadius = standCapsuleRadius * 0.95f;

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

        if (isCrouching) currentSpeed *= crouchSpeedMultiplier;

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(direction.x * currentSpeed, rb.linearVelocity.y, direction.z * currentSpeed);
        }
    }

    public bool IsGrounded => isGrounded;

    public float HorizontalSpeed
    {
        get
        {
            if (rb == null) return 0f;
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            return v.magnitude;
        }
    }

    [ContextMenu("Fetch Standing Values")]
    private void FetchStandingValues()
    {
        CapsuleCollider c = GetComponent<CapsuleCollider>();
        if (c != null)
        {
            standHeight = c.height;
            standCapsuleRadius = c.radius;
            standCenter = c.center;
        }

        if (cameraHolder != null) standCameraPos = cameraHolder.localPosition;
        if (body != null) standBodyPos = body.localPosition;

        Debug.Log("Standing values locked in! Your SFX and Bobbing should work perfectly now.");
    }
}