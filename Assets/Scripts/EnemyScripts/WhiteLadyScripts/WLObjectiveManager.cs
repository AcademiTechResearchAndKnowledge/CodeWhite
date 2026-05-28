using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WLObjectiveManager : MonoBehaviour
{
    public static WLObjectiveManager Instance;
    public RandomPortalSpawner RPS;

    [Header("Mirror Progress")]
    public int totalMirrorPieces = 6;
    public int collectedMirrorPieces = 0;
    private bool hasFixedMirror = false;

    [Header("Inventory Data Link")]
    public ObjectiveItemData brokenMirrorPieceData;
    public ObjectiveItemData fixedMirrorData;

    [Header("Flower Progress")]
    public bool flowerCollected = false;

    [Header("Rewards / Progression")]
    public GameObject fixedMirrorObject;
    public bool progressionUnlocked = false;

    [Header("Entity Reference")]
    public WhiteLady whiteLadyEntity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (RPS == null)
        {
            RPS = FindFirstObjectByType<RandomPortalSpawner>();
        }
    }

    public void CollectMirrorPiece()
    {
        if (progressionUnlocked) return;

        collectedMirrorPieces++;

        if (collectedMirrorPieces >= totalMirrorPieces)
        {
            GiveFixedMirror();
        }
    }

    public void CollectFlower()
    {
        if (progressionUnlocked) return;
        if (flowerCollected) return;

        flowerCollected = true;
    }

    private void GiveFixedMirror()
    {
        if (ObjectiveInventoryManager.Instance != null &&
            brokenMirrorPieceData != null &&
            fixedMirrorData != null)
        {
            ObjectiveInventoryManager.Instance.RemoveItem(brokenMirrorPieceData, totalMirrorPieces);
            ObjectiveInventoryManager.Instance.AddItem(fixedMirrorData, 1);
            hasFixedMirror = true;
        }

        if (fixedMirrorObject != null)
        {
            fixedMirrorObject.SetActive(true);
        }
    }

    public void SubmitFixedMirror()
    {
        if (progressionUnlocked) return;

        if (!hasFixedMirror) return;

        if (ObjectiveInventoryManager.Instance != null && fixedMirrorData != null)
        {
            ObjectiveInventoryManager.Instance.RemoveItem(fixedMirrorData, 1);
        }

        UnlockProgress("Fixed mirror submitted.");
    }

    public void UnlockProgress(string reason)
    {
        if (progressionUnlocked) return;

        progressionUnlocked = true;

        if (whiteLadyEntity != null)
        {
            whiteLadyEntity.Despawn();
        }

        if (RPS != null)
        {
            RPS.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
        }
    }

    public void DebugFinishPuzzle()
    {
        hasFixedMirror = true;
        UnlockProgress("Debug finish puzzle");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WLObjectiveManager))]
public class WLObjectiveManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WLObjectiveManager manager = (WLObjectiveManager)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Finish Puzzle (Debug)"))
        {
            manager.DebugFinishPuzzle();
            EditorUtility.SetDirty(manager);
        }
    }
}
#endif