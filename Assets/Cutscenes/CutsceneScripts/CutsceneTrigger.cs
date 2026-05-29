using UnityEngine;

// This ensures the GameObject has a Collider, which is required for triggers
[RequireComponent(typeof(Collider))]
public class CutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene Connection")]
    [Tooltip("Drag your CutsceneManager here.")]
    public CutsceneManager cutsceneManager;

    [Header("Trigger Settings")]
    [Tooltip("The tag of the object that triggers the cutscene.")]
    public string triggerTag = "Player";

    [Tooltip("If true, the trigger will only fire once and then disable itself.")]
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // Stop if it already triggered and is only supposed to run once
        if (hasTriggered && triggerOnce) return;

        // Check if the colliding object has the correct tag
        if (other.CompareTag(triggerTag))
        {
            if (cutsceneManager != null)
            {
                cutsceneManager.ActivateCutscene();
                hasTriggered = true;
            }
            else
            {
                Debug.LogWarning("CutsceneTrigger is missing a reference to a CutsceneManager!", this);
            }
        }
    }
}