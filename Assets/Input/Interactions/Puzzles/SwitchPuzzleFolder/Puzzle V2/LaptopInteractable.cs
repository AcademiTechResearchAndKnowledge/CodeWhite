using UnityEngine;

public class LaptopInteractable : Interactable
{
    public override void Interact()
    {
        base.Interact(); // fires UnityEvent if needed
        LaptopManager.Instance.ToggleLaptop();
    }
}