using UnityEngine;

public class WLObjectiveManager : MonoBehaviour
{
    public static WLObjectiveManager Instance;

    [Header("Mirror Progress")]
    public int totalMirrorPieces = 6;
    public int collectedMirrorPieces = 0;
    private bool hasFixedMirror = false; // Tracks if the player is holding the crafted mirror

    [Header("Inventory Data Link")]
    public ObjectiveItemData brokenMirrorPieceData;
    public ObjectiveItemData fixedMirrorData;

    [Header("Flower Progress")]
    public bool flowerCollected = false;

    [Header("Rewards / Progression")]
    public GameObject fixedMirrorObject;
    public GameObject portalToOpen;
    public bool progressionUnlocked = false;

    [Header("Entity Reference")]
    [Tooltip("Drag the White Lady GameObject here so the manager can despawn her when the puzzle is done.")]
    public WhiteLady whiteLadyEntity;

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
            // REMOVED: UnlockProgress was automatically firing here. 
            Debug.Log("All pieces collected! Fixed mirror is in inventory. Find the White Lady to give it to her.");
        }
    }

    public void CollectFlower()
    {
        if (progressionUnlocked) return;
        if (flowerCollected) return;

        flowerCollected = true;
        Debug.Log("Flower collected. Find the White Lady to submit it.");
    }

    private void GiveFixedMirror()
    {
        if (ObjectiveInventoryManager.Instance != null && brokenMirrorPieceData != null && fixedMirrorData != null)
        {
            ObjectiveInventoryManager.Instance.RemoveItem(brokenMirrorPieceData, totalMirrorPieces);
            ObjectiveInventoryManager.Instance.AddItem(fixedMirrorData, 1);
            hasFixedMirror = true; // Player now holds the mirror
            Debug.Log("Inventory Updated: Swapped mirror pieces for the fixed mirror.");
        }
        else
        {
            Debug.LogWarning("Missing inventory references in WLObjectiveManager!");
        }

        if (fixedMirrorObject != null)
        {
            fixedMirrorObject.SetActive(true);
        }

        Debug.Log("Fixed mirror granted");
    }

    // --- NEW: Call this method when the player interacts with her hitbox to submit the mirror ---
    public void SubmitFixedMirror()
    {
        if (progressionUnlocked) return;

        // Ensure they actually have the completed mirror first
        if (!hasFixedMirror)
        {
            Debug.Log("You don't have the fixed mirror yet!");
            return;
        }

        // Take the mirror out of their inventory upon turn-in
        if (ObjectiveInventoryManager.Instance != null && fixedMirrorData != null)
        {
            ObjectiveInventoryManager.Instance.RemoveItem(fixedMirrorData, 1);
        }

        UnlockProgress("Fixed mirror successfully given to the White Lady.");
    }

    public void UnlockProgress(string reason)
    {
        if (progressionUnlocked) return;

        progressionUnlocked = true;
        Debug.Log("Progress unlocked: " + reason);

        // White lady now ONLY despawns when this final progression stage is officially unlocked
        if (whiteLadyEntity != null)
        {
            whiteLadyEntity.Despawn();
            Debug.Log("White Lady has been successfully despawned.");
        }
        else
        {
            Debug.LogWarning("White Lady Entity is not assigned in the WLObjectiveManager!");
        }

        if (portalToOpen != null)
        {
            portalToOpen.SetActive(true);
        }
    }
}