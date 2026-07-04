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

    [Header("Cleanup Options")]
    [Tooltip("If true, destroys the object after playing. Best for spawned prefabs.")]
    public bool destroyAfterPlaying = true;

    private bool isJumpscaring = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
        {
            TriggerJumpscare();
        }
    }

    public void TriggerJumpscare()
    {
        if (!isJumpscaring && jumpscareContainer != null && jumpscareAnimator != null)
        {
            StartCoroutine(JumpscareRoutine());
        }
    }

    private IEnumerator JumpscareRoutine()
    {
        isJumpscaring = true;

        BookInspectionUI bookUI = FindFirstObjectByType<BookInspectionUI>();
        if (bookUI != null && bookUI.IsOpen())
        {
            bookUI.CloseInspection();
        }

        jumpscareContainer.SetActive(true);

        if (jumpscareAudio != null)
        {
            jumpscareAudio.Play();
        }

        jumpscareAnimator.SetTrigger(triggerParameterName);

        yield return new WaitForSeconds(animationDuration - 0.5f);

        if (destroyAfterPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            jumpscareContainer.SetActive(false);
            isJumpscaring = false;
        }
    }
}