using UnityEngine;
using UnityEngine.InputSystem;

public class ClosetHideInteract : MonoBehaviour  
{
    public bool CanInteract = true;

    public ClosetHidingSystem HidingSystem;

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame && HidingSystem.InsideCloset == true)
        {
            CanInteract = true;
            HidingSystem.GoOutsideCloset();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame && CanInteract == true)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5)) 
            { 
                if (hit.collider.CompareTag("Closet"))
                {
                    CanInteract = false;
                    HidingSystem.GoInsideCloset();
                }
            }
        }
    }
}
