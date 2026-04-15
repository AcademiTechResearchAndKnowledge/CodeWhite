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

    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchCenterY = 0.5f;

    [SerializeField] private float crouchBodyOffset = -0.5f;
    [SerializeField] private float crouchCameraOffset = -0.5f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;   // For ground
    public LayerMask ceilingMask;  // For ceiling detection

    private Rigidbody rb;
    private CapsuleCollider col;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;
    public bool isCrouching;

    private float normalHeight;
    private Vector3 normalCenter;

    private Vector3 originalBodyPos;
    private Vector3 originalCameraPos;

    private PlayerStats playerStats;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        playerStats = GetComponent<PlayerStats>();

        normalHeight = col.height;
        normalCenter = col.center;

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

        if (isCrouching)
        {
            if (!CanStand())
                return;
            StopCrouch();
        }

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

        bool wantCrouch = Keyboard.current.leftCtrlKey.isPressed && isGrounded;
        bool forcedCrouch = IsCeilingAbove();

        if (wantCrouch || forcedCrouch)
        {
            if (!isCrouching)
                StartCrouch();
        }
        else
        {
            if (isCrouching && CanStand())
                StopCrouch();
        }
    }

    void StartCrouch()
    {
        isCrouching = true;
        col.height = crouchHeight;
        col.center = new Vector3(col.center.x, crouchCenterY, col.center.z);

        if (body != null)
            body.localPosition = originalBodyPos + new Vector3(0, crouchBodyOffset, 0);

        if (cameraHolder != null)
            cameraHolder.localPosition = originalCameraPos + new Vector3(0, crouchCameraOffset, 0);
    }

    void StopCrouch()
    {
        isCrouching = false;
        col.height = normalHeight;
        col.center = normalCenter;

        if (body != null)
            body.localPosition = originalBodyPos;

        if (cameraHolder != null)
            cameraHolder.localPosition = originalCameraPos;
    }

    bool CanStand()
    {
        // Half the height difference between standing and current height
        float halfHeightDiff = (normalHeight - col.height) / 2f;
        Vector3 standCenter = transform.position + Vector3.up * (col.height / 2f + halfHeightDiff);

        // Check if a capsule at standing height would collide (ignore triggers)
        return !Physics.CheckCapsule(
            standCenter + Vector3.up * (normalHeight / 2f - 0.01f),
            standCenter - Vector3.up * (normalHeight / 2f - 0.01f),
            col.radius * 0.95f,
            ceilingMask,
            QueryTriggerInteraction.Ignore
        );
    }

    bool IsCeilingAbove()
    {
        float checkDistance = normalHeight + 0.1f;
        float radius = col.radius * 0.8f;

        // Use collider bounds top — always accurate regardless of center offset
        Vector3 capsuleTop = new Vector3(transform.position.x, col.bounds.max.y + 0.01f, transform.position.z);

        Vector3[] offsets = new Vector3[]
        {
        Vector3.zero,
        transform.forward  * radius,
        -transform.forward * radius,
        transform.right    * radius,
        -transform.right   * radius,
        };

        foreach (Vector3 offset in offsets)
        {
            if (Physics.Raycast(capsuleTop + offset, Vector3.up, out RaycastHit hit, checkDistance, ceilingMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider != col)
                    return true;
            }
        }

        return false;
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