using UnityEngine;

public class SlideScript : MonoBehaviour
{
    [Header("Ice Settings")]
    public float slideAcceleration = 12f;
    public float maxSlideSpeed = 20f;
    public float friction = 0.05f;

    private Rigidbody playerRb;
    private bool isOnIce = false;
    private Vector3 groundNormal;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            playerRb = collision.collider.GetComponent<Rigidbody>();
            isOnIce = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        isOnIce = true;
        playerRb = collision.collider.GetComponent<Rigidbody>();

        ContactPoint contact = collision.GetContact(0);
        groundNormal = contact.normal;

        ApplyIcePhysics();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isOnIce = false;
            playerRb = null;
        }
    }

    private void ApplyIcePhysics()
    {
        if (playerRb == null) return;

        Vector3 velocity = playerRb.linearVelocity;

        Vector3 gravity = Physics.gravity;
        Vector3 slideDirection = Vector3.ProjectOnPlane(gravity, groundNormal).normalized;

        if (slideDirection.magnitude > 0.01f)
        {
            playerRb.AddForce(slideDirection * slideAcceleration, ForceMode.Acceleration);
        }

        velocity *= (1f - friction);

        if (velocity.magnitude > maxSlideSpeed)
        {
            velocity = velocity.normalized * maxSlideSpeed;
        }

        playerRb.linearVelocity = velocity;
    }
}