using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class NewTutorialTrigger : MonoBehaviour
{
    [Header("Trigger Identification")]
    [Tooltip("Must match the exact string in the manager (e.g., 'StartBarrier')")]
    public string triggerID;

    [Tooltip("The ID of the dialogue you want this barrier to play from the DialogueDatabase")]
    public string dialogueID;

    [Header("Optional Events")]
    [Tooltip("Play a jump scare scream audio or run animation routines here.")]
    public UnityEvent onTriggerEntered;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;

            onTriggerEntered?.Invoke();

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.HandleBarrierTriggered(triggerID, dialogueID);
            }
        }
    }
}