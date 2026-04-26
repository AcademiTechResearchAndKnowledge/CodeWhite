using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline))]
public class Interactable : MonoBehaviour
{
    private Outline outline;

    [Header("UI Prompts")]
    [Tooltip("The key/button to press (e.g., E, F, Mouse1)")]
    public string buttonText = "E";

    [Tooltip("What is this object? (e.g., Door, Closet, Document)")]
    public string objectName = "Object";

    [Tooltip("What happens when interacted with? (e.g., Open, Hide, Read)")]
    public string actionName = "Interact";

    [Header("Interaction Event")]
    [Tooltip("Drag the script/method here that should run when the player interacts.")]
    public UnityEvent onInteraction;

    private void Awake()
    {
        outline = GetComponent<Outline>();

        if (outline == null)
        {
            Debug.LogWarning("No Outline component found on " + gameObject.name);
            return;
        }

        DisableOutline();
    }

    public virtual void Interact()
    {
        onInteraction?.Invoke();
    }

    public void DisableOutline()
    {
        if (outline != null)
            outline.enabled = false;
    }

    public void EnableOutline()
    {
        if (outline != null)
            outline.enabled = true;
    }
}