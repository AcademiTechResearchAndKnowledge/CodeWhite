using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float playerReach = 3f;

    private Camera _cam;
    private Interactable currentInteractable;

    void Start()
    {
        _cam = Camera.main;

        if (_cam == null)
            Debug.LogError("PlayerInteraction: No camera tagged 'MainCamera' found.", this);
    }

    void Update()
    {
        CheckInteraction();

        if (Keyboard.current.fKey.wasPressedThisFrame && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckInteraction()
    {
        if (_cam == null) return;

        RaycastHit hit;
        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out hit, playerReach))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                Interactable newInteractable = hit.collider.GetComponentInParent<Interactable>();

                if (newInteractable == null)
                {
                    DisableCurrentInteractable();
                    return;
                }

                if (currentInteractable != null && newInteractable != currentInteractable)
                {
                    currentInteractable.DisableOutline();
                }

                if (newInteractable.enabled)
                {
                    SetNewCurrentInteractable(newInteractable);
                }
                else
                {
                    DisableCurrentInteractable();
                }
            }
            else
            {
                DisableCurrentInteractable();
            }
        }
        else
        {
            DisableCurrentInteractable();
        }
    }

    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        if (currentInteractable == newInteractable) return;

        if (currentInteractable != null)
        {
            currentInteractable.DisableOutline();
        }

        currentInteractable = newInteractable;
        currentInteractable.EnableOutline();
        HUDInteractController.Instance.EnableInteractionText(currentInteractable.message);
    }

    void DisableCurrentInteractable()
    {
        HUDInteractController.Instance.DisableInteractionText();
        if (currentInteractable != null)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
    }
}