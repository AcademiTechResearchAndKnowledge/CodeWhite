using UnityEngine;

public class SpriteDirectionalController : MonoBehaviour
{
    [Header("Angle Thresholds (Perfect 4-Way)")]
    [SerializeField] float frontAngle = 45f;
    [SerializeField] float backAngle = 135f;

    [Header("References")]
    [SerializeField] Transform targetPlayer;
    [SerializeField] Transform body;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void Update()
    {
        Vector3 directionToPlayer = targetPlayer.position - body.position;
        directionToPlayer.y = 0f;
        directionToPlayer.Normalize();

        Vector3 bodyForwardVector = body.forward;
        bodyForwardVector.y = 0f;
        bodyForwardVector.Normalize();

        float signedAngle = Vector3.SignedAngle(bodyForwardVector, directionToPlayer, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        Vector2 animationDirection = Vector2.zero;

        // Front
        if (angle <= frontAngle)
        {
            animationDirection = new Vector2(0f, 1f);
            spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        // Side
        else if (angle < backAngle)
        {
            animationDirection = new Vector2(1f, 0f);

            // --- THE FIX IS HERE ---
            // We swapped the 1 and -1 around!
            if (signedAngle < -0.1f)
            {
                spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (signedAngle > 0.1f)
            {
                spriteRenderer.transform.localScale = new Vector3(-1f, 1f, 1f);
            }
        }
        // Back
        else
        {
            animationDirection = new Vector2(0f, -1f);
            spriteRenderer.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        animator.SetFloat("moveX", animationDirection.x);
        animator.SetFloat("moveY", animationDirection.y);
    }
}