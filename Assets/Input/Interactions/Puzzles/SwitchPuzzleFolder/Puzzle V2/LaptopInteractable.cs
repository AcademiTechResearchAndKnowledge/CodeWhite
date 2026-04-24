using UnityEngine;
using System.Collections; // Required for the Coroutine!

// FOR THE LAPTOP OBJECT, SO THE PLAYER CAN INTERACT WITH IT
public class LaptopInteractable : Interactable
{
    void Start()
    {
        // Start the timer to shut down the aggressive QuickOutline script on the laptop
        StartCoroutine(KillOutlineAfterSetup());
    }

    private IEnumerator KillOutlineAfterSetup()
    {
        // Wait exactly 1 frame for QuickOutline to finish turning itself on
        yield return new WaitForEndOfFrame();

        // Safely shut it down so the laptop doesn't glow when the game starts!
        DisableOutline();
    }

    public override void Interact()
    {
        base.Interact(); // fires UnityEvent if needed
        LaptopManager.Instance.ToggleLaptop();
    }
}