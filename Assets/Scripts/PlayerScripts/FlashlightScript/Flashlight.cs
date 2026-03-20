using UnityEngine;
using UnityEngine.InputSystem;

public class Flashlight : MonoBehaviour
{
    [SerializeField] private InputActionReference toggleAction;
    [SerializeField] private Light torchLight;

    private void Awake()
    {
        torchLight.enabled = false;
    }

    private void OnEnable()
        {
            toggleAction.action.performed += OnToggle;
        }
        
    private void OnDisable()
        {
            toggleAction.action.performed -= OnToggle;
        }
    
    private void OnToggle(InputAction.CallbackContext ctx)
        {
            torchLight.enabled = !torchLight.enabled;
        }
}