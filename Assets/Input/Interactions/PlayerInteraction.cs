using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInteraction : MonoBehaviour
{
    public float playerReach = 3f;

    private Camera _cam;
    private Interactable currentInteractable;
    private HUDInteractController hud;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        _cam = Camera.main;

        if (_cam == null)
            Debug.LogError("No MainCamera found");

        hud = GetHUD();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded - resetting interaction system");

        currentInteractable = null;
        hud = null;

        _cam = Camera.main;
        hud = GetHUD();
    }

    void Update()
    {
        CheckInteraction();

        if (Keyboard.current != null &&
            Keyboard.current.fKey.wasPressedThisFrame &&
            currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void CheckInteraction()
    {
        if (_cam == null) return;

        Ray ray = new Ray(_cam.transform.position, _cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, playerReach))
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
                    currentInteractable.DisableOutline();

                if (newInteractable.enabled)
                    SetNewCurrentInteractable(newInteractable);
                else
                    DisableCurrentInteractable();
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
            currentInteractable.DisableOutline();

        currentInteractable = newInteractable;
        currentInteractable.EnableOutline();

        hud = GetHUD();
        if (hud == null)
            return;

        hud.EnableInteractionText(
            currentInteractable.buttonText,
            currentInteractable.objectName,
            currentInteractable.actionName
        );
    }

    void DisableCurrentInteractable()
    {
        hud = GetHUD();

        if (hud != null)
            hud.DisableInteractionText();

        if (currentInteractable != null)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
    }

    HUDInteractController GetHUD()
    {
        if (hud != null)
            return hud;

        if (HUDInteractController.Instance != null)
        {
            hud = HUDInteractController.Instance;
            return hud;
        }

        HUDInteractController found = Object.FindFirstObjectByType<HUDInteractController>();

        if (found != null)
        {
            HUDInteractController.Instance = found;
            hud = found;
            Debug.Log("HUD re-linked");
            return hud;
        }

        Debug.Log("HUD not found");
        return null;
    }
}