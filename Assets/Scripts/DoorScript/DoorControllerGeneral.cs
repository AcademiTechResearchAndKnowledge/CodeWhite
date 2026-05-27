using UnityEngine;
using System.Collections;

public class DoorControllerGeneral : Interactable
{
    [Header("Animation")]
    public Animator doorAnimator;

    [Tooltip("How long to wait before switching to the Idle state after opening/closing.")]
    public float animationStepDelay = 0.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip doorOpenSound;
    [Tooltip("Delay in seconds before the open sound plays.")]
    public float openSoundDelay = 0f;

    public AudioClip doorCloseSound;
    [Tooltip("Delay in seconds before the close sound plays.")]
    public float closeSoundDelay = 0f;

    private bool isOpen = false;
    private bool isBusy = false;

    private int aiInZone = 0;
    private TutorialManager tutorialManager;

    private void Start()
    {
        tutorialManager = FindAnyObjectByType<TutorialManager>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (aiInZone > 0 && !isOpen && !isBusy)
        {
            Debug.Log($"[{gameObject.name}] AI is blocking the door! Forcing it open.");
            StartCoroutine(OpenDoorRoutine());
        }
    }

    public override void Interact()
    {
        if (isBusy) return;

        base.Interact();

        if (tutorialManager != null)
        {
            tutorialManager.DoorInteracted();
        }

        if (doorAnimator == null)
        {
            Debug.LogWarning("No Animator assigned on " + gameObject.name);
            return;
        }

        if (!isOpen)
        {
            StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            StartCoroutine(CloseDoorRoutine());
        }
    }

    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    //  AI Trigger Detector (Automatic Doors)
    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<WhiteLady>() != null ||
            other.GetComponentInParent<AggroEntityDetector>() != null ||
            other.GetComponentInParent<StalkerFollowScript>() != null ||
            other.GetComponentInParent<DespawningEntityDetector>() != null)
        {
            aiInZone++;

            if (!isOpen && !isBusy)
            {
                StartCoroutine(OpenDoorRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<WhiteLady>() != null ||
            other.GetComponentInParent<AggroEntityDetector>() != null ||
            other.GetComponentInParent<StalkerFollowScript>() != null ||
            other.GetComponentInParent<DespawningEntityDetector>() != null)
        {
            aiInZone--;

            if (aiInZone <= 0 && isOpen && !isBusy)
            {
                StartCoroutine(CloseDoorRoutine());
            }
        }
    }

    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
    //  Coroutines & Audio
    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

    IEnumerator OpenDoorRoutine()
    {
        isBusy = true;
        PlaySound(doorOpenSound, openSoundDelay);

        doorAnimator.SetInteger("C", 1);
        yield return new WaitForSeconds(animationStepDelay);
        doorAnimator.SetInteger("C", 2);

        isOpen = true;
        isBusy = false;
    }

    IEnumerator CloseDoorRoutine()
    {
        isBusy = true;
        PlaySound(doorCloseSound, closeSoundDelay);

        doorAnimator.SetInteger("C", 3);
        yield return new WaitForSeconds(animationStepDelay);
        doorAnimator.SetInteger("C", 4);

        isOpen = false;
        isBusy = false;
    }

    private void PlaySound(AudioClip clip, float delay = 0f)
    {
        if (audioSource != null && clip != null)
        {
            if (delay > 0f)
            {
                StartCoroutine(PlaySoundCO(clip, delay));
            }
            else
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    private IEnumerator PlaySoundCO(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}