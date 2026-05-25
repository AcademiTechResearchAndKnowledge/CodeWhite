using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class TutorialTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("Unique name to identify this trigger in the TutorialManagerNew (e.g., 'DarkBarrier', 'ScreamBarrier')")]
    public string triggerID;
    public string dialogueIDToPlay;

    [Header("Optional Events")]
    [Tooltip("Use this to play the Scream Audio or trigger animations when the player hits this barrier.")]
    public UnityEvent onTriggerEntered;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Ensure only the player triggers this, and it only happens once
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            // Fire off any custom events (like playing the scream audio)
            onTriggerEntered?.Invoke();

            // Tell the TutorialManagerNew we hit this barrier
            if (TutorialManagerNew.Instance != null)
            {
                TutorialManagerNew.Instance.HandleBarrierTriggered(triggerID, dialogueIDToPlay);
            }
        }
    }
}