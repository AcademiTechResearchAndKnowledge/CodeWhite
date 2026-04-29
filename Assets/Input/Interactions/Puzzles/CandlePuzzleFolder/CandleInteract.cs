using UnityEngine;

public class CandleInteract : Interactable
{
    [Header("Puzzle Settings")]
    [Tooltip("Drag the Data_Lighter ScriptableObject here so the candle knows what to look for.")]
    public ObjectiveItemData lighterData;

    [Tooltip("The flame particle system or light object to turn on when lit.")]
    public GameObject flameVisual;

    [Header("Audio Settings")]
    [Tooltip("The AudioSource component attached to this candle.")]
    public AudioSource audioSource;
    [Tooltip("The sound effect for lighting the candle.")]
    public AudioClip lightSound;
    [Tooltip("The sound effect for when the candle is blown out.")]
    public AudioClip extinguishSound;

    private bool isLit = false;

    private void Start()
    {
        // Ensure the flame is off when the game starts
        if (flameVisual != null)
        {
            flameVisual.SetActive(false);
        }
    }

    public override void Interact() // This runs when you press 'F' on the candle
    {
        if (isLit)
        {
            Debug.Log("This candle is already lit.");
            return;
        }

        // 1. Get the item the player is currently holding
        ObjectiveInventorySlot selectedSlot = ObjectiveInventoryManager.Instance.GetSelectedSlot();

        // 2. Check if the slot is valid, not empty, and holds the exact Lighter data
        if (selectedSlot == null || selectedSlot.IsEmpty() || selectedSlot.item != lighterData)
        {
            Debug.Log("You need to have the lighter equipped in your hand to light this!");
            return; // Stop the code here
        }

        // 3. If we reach this point, the player has the lighter equipped. Light it!
        isLit = true;
        if (flameVisual != null)
        {
            flameVisual.SetActive(true);
        }

        // Play the lighting sound effect
        if (audioSource != null && lightSound != null)
        {
            audioSource.PlayOneShot(lightSound);
        }

        Debug.Log("Candle lit successfully!");

        // 4. Notify the Puzzle Manager
        if (LighterPuzzleManager.instance != null)
        {
            LighterPuzzleManager.instance.CandleLit(this);
        }

        // 5. Consume (Remove) the lighter from the inventory
        ObjectiveInventoryManager.Instance.RemoveItem(lighterData, 1);

        // Clear UI text if necessary
        if (HUDInteractController.Instance != null)
        {
            HUDInteractController.Instance.DisableInteractionText();
        }
    }

    // This method is already called by your LighterPuzzleManager's BlowOutCandle() method
    public void Extinguish()
    {
        isLit = false;
        if (flameVisual != null)
        {
            flameVisual.SetActive(false);
        }

        // Play the extinguish sound effect
        if (audioSource != null && extinguishSound != null)
        {
            audioSource.PlayOneShot(extinguishSound);
        }

        Debug.Log("The candle flame was extinguished.");
    }
}