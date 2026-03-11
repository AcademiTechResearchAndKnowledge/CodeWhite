using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Transform body;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    [SerializeField] private float sprintMultiplier = 1.3f;
    [SerializeField] private float exhaustedMultiplier = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;

    private PlayerStats playerStats;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerStats = GetComponent<PlayerStats>();

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on Player.");
        }
    }

    void Update()
    {
        CheckGround();

        isSprinting = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

        HandleStamina();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void OnJump()
    {
        if (isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }

    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void HandleStamina()
    {
        if (playerStats == null) return;

        bool isActuallyMoving = HorizontalSpeed > 0.1f;

        if (isSprinting && isActuallyMoving && playerStats.HasStamina())
        {
            playerStats.DrainStamina(playerStats.GetDrainRate() * Time.deltaTime);
            Debug.Log("Draining Stamina | Current Stamina: " + playerStats.Stamina);
        }
        else if (!isSprinting)
        {
            playerStats.RegenerateStamina(playerStats.GetRegenRate() * Time.deltaTime);
            Debug.Log("Regenerating Stamina | Current Stamina: " + playerStats.Stamina);
        }
        else if (isSprinting && !isActuallyMoving)
        {
            Debug.Log("Stamina Frozen | Current Stamina: " + playerStats.Stamina);
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
            {
                currentSpeed = moveSpeed * exhaustedMultiplier;
            }
            else if (isSprinting && isMoving)
            {
                currentSpeed = moveSpeed * sprintMultiplier;
            }
        }

        rb.linearVelocity = new Vector3(
            direction.x * currentSpeed,
            rb.linearVelocity.y,
            direction.z * currentSpeed
        );
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