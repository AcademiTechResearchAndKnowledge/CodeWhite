using UnityEngine;
using UnityEngine.InputSystem;

public class SpriteDirectional : MonoBehaviour
{
    [SerializeField] float backAngle = 65f;
    [SerializeField] float sideAngle = 155f;
    [SerializeField] Transform body;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    private Vector3 lastFacingDirection = Vector3.forward;

    private void LateUpdate()
    {
        Vector3 camForwardVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;

        float signedAngle = Vector3.SignedAngle(lastFacingDirection, camForwardVector, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        Vector2 animationDirection = new Vector2(0f, -1f);

        if (angle < backAngle)
        {
            // Back animation
            animationDirection = new Vector2(0f, -1f);

            // RESET SCALE to normal
            spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (angle < sideAngle)
        {
            // Side animation
            animationDirection = new Vector2(1f, 0f);

            // --- THE SHADER-PROOF FLIP ---
            if (signedAngle < 0f)
            {
                // Flip the sprite visually by turning it inside out (Scale X to -1)
                spriteRenderer.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else
            {
                // Face right normally (Scale X to 1)
                spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
        else
        {
            // Front animation
            animationDirection = new Vector2(0f, 1f);

            // RESET SCALE to normal
            spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        animator.SetFloat("moveX", animationDirection.x);
        animator.SetFloat("moveY", animationDirection.y);
    }
}