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

        yield return new WaitForEndOfFrame();

       
        DisableOutline();
    }

    public override void Interact()
    {
        base.Interact(); 
        LaptopManager.Instance.StartInteraction();
    }
}