using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Transform body;

    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public int anxiety = 0;
    public int lightRange = 0;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private PlayerInput playerInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = new PlayerInput();
    }

    void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    void OnJump ()
    {
        if (isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void OnMovement(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void MovePlayer ()
    {
        Vector3 direction = body.right * moveInput.x + body.forward * moveInput.y;
        direction.Normalize();
        rb.linearVelocity = new Vector3 (direction.x * moveSpeed, rb.linearVelocity.y,direction.z * moveSpeed);
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
