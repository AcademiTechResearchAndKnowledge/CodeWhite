using UnityEngine;

public class PlayerBobble : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;

    [Header("Bob Settings")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;

    [Header("Optional Sway (X/Z)")]
    public float swayX = 0.02f;
    public float swayZ = 0.02f;

    [Header("Smoothing")]
    public float smooth = 12f;

    private Vector3 startLocalPos;
    private float timer;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (playerMovement == null)
            return;

        bool grounded = playerMovement.IsGrounded;
        float speed = playerMovement.HorizontalSpeed;
        bool moving = speed > 0.1f;

        // Only bob when moving on ground
        if (!grounded || !moving)
        {
            timer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, startLocalPos, Time.deltaTime * smooth);
            return;
        }

        timer += Time.deltaTime * walkBobSpeed;

        float y = Mathf.Sin(timer) * walkBobAmount;
        float x = Mathf.Cos(timer * 0.5f) * swayX;
        float z = Mathf.Sin(timer * 0.5f) * swayZ;

        Vector3 target = startLocalPos + new Vector3(x, y, z);
        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * smooth);
    }
}