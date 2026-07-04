using UnityEngine;

public interface IZoomInteractable
{
    bool IsInteracting { get; set; }

    void StartInteraction();
    void StopInteraction();
}