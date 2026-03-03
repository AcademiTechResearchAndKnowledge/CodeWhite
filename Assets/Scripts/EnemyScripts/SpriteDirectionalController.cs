using UnityEngine;

public class SpriteDirectionalController : MonoBehaviour
{
    [SerializeField] float backAngle = 65f;
    [SerializeField] float sideAngle = 155f;
    [SerializeField] Transform mainTransform;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;

    private void LateUpdate()
    {
        Vector3 camForwardVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        Debug.DrawRay(Camera.main.transform.position, camForwardVector * 5f, Color.magenta);

        float signedAngle = Vector3.SignedAngle(mainTransform.forward, camForwardVector, Vector3.up);

        Vector2 animationDirection = new Vector2(0f, -1f);

        float angle = Mathf.Abs(signedAngle);

        if (angle < backAngle)
        {
            // Back animation
            animationDirection = new Vector2(0f, -1f);
        }
        else if (angle < sideAngle)
        {
            // Side animation (right animation in this case)
            animationDirection = new Vector2(1f, 0f);

            if (signedAngle < 0f)
            {
                spriteRenderer.flipX = true;
            }
            else
            {
                spriteRenderer.flipX = false;
            }
        }
        else
        {
            // Front animation
            animationDirection = new Vector2(0f, 1f);
        }

        animator.SetFloat("moveX", animationDirection.x);
        animator.SetFloat("moveY", animationDirection.y);
    }


}
