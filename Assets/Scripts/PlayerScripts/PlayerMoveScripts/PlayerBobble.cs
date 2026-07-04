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
        // 1. Prevent silent failures
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerBobble is missing a reference to PlayerMovement!");
            return;
        }

        bool grounded = playerMovement.IsGrounded;
        float speed = playerMovement.HorizontalSpeed;
        bool moving = speed > 0.1f;

        Vector3 targetPos = startLocalPos;

        // 2. Only bob when moving on ground
        if (grounded && moving)
        {
            timer += Time.deltaTime * walkBobSpeed;

            float y = Mathf.Sin(timer) * walkBobAmount;
            float x = Mathf.Cos(timer * 0.5f) * swayX;
            float z = Mathf.Sin(timer * 0.5f) * swayZ;

            // Apply directly instead of lerping the moving target, preventing amplitude dampening
            targetPos = startLocalPos + new Vector3(x, y, z);
            transform.localPosition = targetPos;
        }
        else
        {
            // 3. Smoothly return to the start position when stopped
            timer = 0f;
            transform.localPosition = Vector3.Lerp(transform.localPosition, startLocalPos, Time.deltaTime * smooth);
        }
    }
}