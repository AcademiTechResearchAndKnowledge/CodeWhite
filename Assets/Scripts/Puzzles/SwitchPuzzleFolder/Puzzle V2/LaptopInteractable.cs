using UnityEngine;

// FOR THE LAPTOP OBJECT, SO THE PLAYER CAN INTERACT WITH IT
public class LaptopInteractable : Interactable
{
    public override void Interact()
    {
        base.Interact(); // fires UnityEvent if needed
        LaptopManager.Instance.ToggleLaptop();
    }
}