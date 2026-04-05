using UnityEngine;

public class PlayerDisableCutscene : MonoBehaviour
{

    public static PlayerDisableCutscene Instance;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Flashlight flashlight;
    [SerializeField] private PlayerLook playerLook;

    private void Awake()
    {
        Instance = this;
    }

    public void Activate() 
    { 
        playerMovement.enabled = true;
        playerInteraction.enabled = true;
        flashlight.enabled = true;
        playerLook.enabled = true;
    }

    public void Deactivate() 
    { 
        playerMovement.enabled = false;
        playerInteraction.enabled = false;
        flashlight.enabled = false;
        playerLook.enabled = false;
    }
}
