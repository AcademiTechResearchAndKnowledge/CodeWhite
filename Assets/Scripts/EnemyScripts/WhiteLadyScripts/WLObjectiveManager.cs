using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WLObjectiveManager : MonoBehaviour
{
    public static WLObjectiveManager Instance;
    public RandomPortalSpawner RPS;

    public int totalMirrorPieces = 6;
    public int collectedMirrorPieces = 0;
    private bool hasFixedMirror = false;

    public ObjectiveItemData brokenMirrorPieceData;
    public ObjectiveItemData fixedMirrorData;

    public bool flowerCollected = false;
    public GameObject fixedMirrorObject;
    public bool progressionUnlocked = false;

    public WhiteLady whiteLadyEntity;
    public string overridePortalDestination;

    private string lastScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        RPS = FindFirstObjectByType<RandomPortalSpawner>();

        lastScene = SceneManager.GetActiveScene().name;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != lastScene)
        {
            ResetAll();
        }

        lastScene = scene.name;
    }

    private void ResetAll()
    {
        collectedMirrorPieces = 0;
        hasFixedMirror = false;
        flowerCollected = false;
        progressionUnlocked = false;

        if (fixedMirrorObject != null)
            fixedMirrorObject.SetActive(false);
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
            if (!string.IsNullOrEmpty(overridePortalDestination))
            {
                RPS.SetForcedSceneOverride(overridePortalDestination);
            }

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