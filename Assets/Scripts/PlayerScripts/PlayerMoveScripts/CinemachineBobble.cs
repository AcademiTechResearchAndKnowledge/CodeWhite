using Unity.Cinemachine;
using System;
using UnityEngine;

[SaveDuringPlay]
[AddComponentMenu("")]
public class CinemachineBobble : CinemachineExtension
{
    [Header("References")]
    public PlayerMovement playerMovement;

    // EVENT: Other scripts can listen to this!
    public event Action OnStep;

    [Header("Dynamic Bob Speeds")]
    public float crouchBobSpeed = 8f;
    public float walkBobSpeed = 14f;
    public float sprintBobSpeed = 18f; // The camera will bob faster here

    [Header("Bob Settings")]
    public float walkBobAmount = 0.05f;
    public float swayX = 0.02f;
    public float swayZ = 0.02f;
    public float smooth = 12f;

    private float timer;
    private Vector3 currentBobOffset = Vector3.zero;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Aim)
        {
            if (playerMovement == null) return;

            Vector3 targetBobOffset = Vector3.zero;

            bool grounded = playerMovement.IsGrounded;
            float speed = playerMovement.HorizontalSpeed;
            bool moving = speed > 0.1f;

            // Set up your thresholds here based on how your PlayerMovement script works
            bool isSprinting = speed > 5f;
            bool isCrouching = speed < 3f;

            if (grounded && moving)
            {
                // 1. Pick the right speed
                float currentSpeed = walkBobSpeed;
                if (isSprinting) currentSpeed = sprintBobSpeed;
                else if (isCrouching) currentSpeed = crouchBobSpeed;

                // 2. Track the timer BEFORE we add to it
                float previousTimer = timer;
                timer += deltaTime * currentSpeed;

                // 3. THE MAGIC MATH: A full wave is 2*PI. A single step is PI.
                // If the timer crosses a multiple of PI, we just completed a step!
                if (Mathf.FloorToInt(timer / Mathf.PI) > Mathf.FloorToInt(previousTimer / Mathf.PI))
                {
                    // Fire the event for any script listening
                    OnStep?.Invoke();
                }

                float y = Mathf.Sin(timer) * walkBobAmount;
                float x = Mathf.Cos(timer * 0.5f) * swayX;
                float z = Mathf.Sin(timer * 0.5f) * swayZ;

                targetBobOffset = new Vector3(x, y, z);
            }
            else
            {
                timer = 0f;
            }

            currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, deltaTime * smooth);
            state.PositionCorrection += state.RawOrientation * currentBobOffset;
        }
    }
}