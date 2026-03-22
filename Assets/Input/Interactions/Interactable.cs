using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Outline))]
public class Interactable : MonoBehaviour
{
    private Outline outline;
    public string message;
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

    public void Interact()
    {
        Debug.Log("Interact called on " + gameObject.name);
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