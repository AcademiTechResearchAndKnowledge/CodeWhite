using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance;

    [Header("Drawers")]
    public DrawerInteract[] drawers;

    [Header("Lighter Prefab")]
    public GameObject lighterPrefab;

    public DrawerInteract currentDrawer;

    [Header("Puzzle Settings")]
    public int candlesLit = 0;
    public int candlesToFinish = 3;

    private GameObject spawnedLighter;

    // Track previous drawer to avoid repeats
    private DrawerInteract previousDrawer;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        RandomizeDrawer();
    }

    public void RandomizeDrawer()
    {
        if (drawers.Length == 0) return;

        int randomIndex;

        // Keep picking a new drawer until it's different from the previous one
        do
        {
            randomIndex = Random.Range(0, drawers.Length);
        } 
        while (drawers[randomIndex] == previousDrawer && drawers.Length > 1);

        previousDrawer = drawers[randomIndex];
        currentDrawer = drawers[randomIndex];

        Debug.Log("Correct drawer is: " + currentDrawer.name);

        // Spawn the lighter on the new drawer
        SpawnLighter(currentDrawer.transform.position, Quaternion.identity);
    }

    public void SpawnLighter(Vector3 position, Quaternion rotation)
    {
        if (lighterPrefab == null)
        {
            Debug.LogError("Lighter prefab not set in PuzzleManager!");
            return;
        }

        if (spawnedLighter != null)
            Destroy(spawnedLighter);

        spawnedLighter = Instantiate(lighterPrefab, position, rotation);
        spawnedLighter.tag = "Interactable";

        Debug.Log("Lighter spawned on drawer: " + currentDrawer.name);
    }

    public void CandleLit()
    {
        candlesLit++;
        Debug.Log("Candles lit: " + candlesLit);

        if (candlesLit >= candlesToFinish)
        {
            PuzzleFinished();
            return;
        }

        RandomizeDrawer();
    }

    void PuzzleFinished()
    {
        Debug.Log("Puzzle Finished! All candles are lit!");
    }
}