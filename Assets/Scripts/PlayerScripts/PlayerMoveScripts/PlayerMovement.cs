using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Transform body;
    
    public float moveSpeed;
    public float jumpForce;
    public float stamina;

    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private PlayerStatus playerStatus;
    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private PlayerInput playerInput;
    
    void Start()
    {
        playerStatus = GameObject.Find("Player").GetComponent<PlayerStatus>();
        stamina = playerStatus.getStat("STA");
        moveSpeed = playerStatus.getStat("SPD");
        jumpForce = playerStatus.getStat("JMP");
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = new PlayerInput();
    }

    void Update()
    {
        if (Convert.ToBoolean(this.moveInput[0]) || Convert.ToBoolean(this.moveInput[1])
            && this.stamina != 0) {
            this.stamina -= 0.02f;
            this.moveSpeed -= 0.0010f;
        } else if (this.stamina < playerStatus.getStat("STA") && this.moveSpeed < playerStatus.getStat("SPD")) {
            if (this.stamina < 0 || this.moveSpeed < 0)
            {
                this.moveInput = new Vector2(0,0);
                this.stamina = 0;
                this.moveSpeed = 0;
            }
            this.stamina += 0.1f;
            this.moveSpeed += 0.005f;
        }
        print(stamina);
        print(moveSpeed);
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
        this.moveInput = value.Get<Vector2>();
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
