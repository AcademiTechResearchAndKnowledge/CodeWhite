using UnityEngine;

public class WLObjectiveManager : MonoBehaviour
{
    public static WLObjectiveManager Instance;

    [Header("Mirror Progress")]
    public int totalMirrorPieces = 6;
    public int collectedMirrorPieces = 0;

    [Header("Inventory Data Link")]
    public ObjectiveItemData brokenMirrorPieceData; // Drag your Mirror Piece data here
    public ObjectiveItemData fixedMirrorData;       // Drag your Fixed Mirror data here

    [Header("Flower Progress")]
    public bool flowerCollected = false;

    [Header("Rewards / Progression")]
    public GameObject fixedMirrorObject;
    public GameObject portalToOpen;
    public bool progressionUnlocked = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void CollectMirrorPiece()
    {
        if (progressionUnlocked) return;

        collectedMirrorPieces++;
        Debug.Log($"Mirror pieces: {collectedMirrorPieces}/{totalMirrorPieces}");

        if (collectedMirrorPieces >= totalMirrorPieces)
        {
            GiveFixedMirror();
            UnlockProgress("All mirror pieces collected");
        }
    }

    public void CollectFlower()
    {
        if (progressionUnlocked) return;
        if (flowerCollected) return;

        flowerCollected = true;
        Debug.Log("Flower collected");

        UnlockProgress("Flower collected");
    }

    private void GiveFixedMirror()
    {
        // --- NEW INVENTORY SWAP LOGIC ---
        if (ObjectiveInventoryManager.Instance != null && brokenMirrorPieceData != null && fixedMirrorData != null)
        {
            // 1. Remove all 6 broken pieces from the inventory
            ObjectiveInventoryManager.Instance.RemoveItem(brokenMirrorPieceData, totalMirrorPieces);

            // 2. Add the 1 fixed mirror to the inventory
            ObjectiveInventoryManager.Instance.AddItem(fixedMirrorData, 1);

            Debug.Log("Inventory Updated: Swapped mirror pieces for the fixed mirror.");
        }
        else
        {
            Debug.LogWarning("Missing inventory references in WLObjectiveManager!");
        }
        // --------------------------------

        if (fixedMirrorObject != null)
        {
            fixedMirrorObject.SetActive(true);
        }

        Debug.Log("Fixed mirror granted");
    }

    //Placeholder
    private void UnlockProgress(string reason)
    {
        if (progressionUnlocked) return;

        progressionUnlocked = true;
        Debug.Log("Progress unlocked: " + reason);

        if (portalToOpen != null)
        {
            portalToOpen.SetActive(true);
        }
    }
}