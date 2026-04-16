using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class JumpscareMechanic : MonoBehaviour
{
    [Header("Jumpscare References")]
    public GameObject jumpscareContainer;
    public Animator jumpscareAnimator;
    public AudioSource jumpscareAudio;

    [Header("Animation Settings")]
    public string triggerParameterName = "PlayJumpscare";
    public float animationDuration = 1.0f;

    private bool isJumpscaring = false;

    void Start()
    {
        // Hide the entire container on start
        if (jumpscareContainer != null)
        {
            jumpscareContainer.SetActive(false);
        }
    }

    void Update()
    {
        // Listen for the "U" key
        if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
        {
            TriggerJumpscare();
        }
    }

    public void TriggerJumpscare()
    {
        // Only trigger if we aren't already jumpscaring
        if (!isJumpscaring && jumpscareContainer != null && jumpscareAnimator != null)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    private IEnumerator JumpscareRoutine()
    {
        isJumpscaring = true;

        // 1. Activate the parent container
        jumpscareContainer.SetActive(true);

        // 2. Play sound if assigned
        if (jumpscareAudio != null)
        {
            jumpscareAudio.Play();
        }

        // 3. Fire the trigger on the Animator
        jumpscareAnimator.SetTrigger(triggerParameterName);

        // 4. THE FIX: Wait out the animation MINUS 0.05 seconds. 
        // This hides the canvas 3-4 frames *before* the Animator snaps back to the T-pose.
        // The player won't notice the missing microsecond, but it completely removes the visual glitch.
        yield return new WaitForSeconds(animationDuration - 0.5f);

        // 5. Hide the container
        jumpscareContainer.SetActive(false);
        isJumpscaring = false;
    }
}