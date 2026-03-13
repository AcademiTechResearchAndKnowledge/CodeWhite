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
        int randomIndex = Random.Range(0, drawers.Length);
        currentDrawer = drawers[randomIndex];

        Debug.Log("Correct drawer is: " + currentDrawer.name);
    }

    public void SpawnLighter(Vector3 position, Quaternion rotation)
    {
        if (lighterPrefab == null)
        {
            Debug.LogError("Lighter prefab not set in PuzzleManager!");
            return;
        }

        // Instantiate the lighter prefab
        GameObject lighter = Instantiate(lighterPrefab, position, rotation);
        lighter.tag = "Interactable";

        Debug.Log("Lighter spawned on drawer: " + currentDrawer.name);
    }

    // Call this when a candle is lit
    public void CandleLit()
    {
        candlesLit++;
        Debug.Log("Candles lit: " + candlesLit);

        if (candlesLit >= candlesToFinish)
        {
            PuzzleFinished();
            return;
        }

        // Choose new drawer for next lighter
        RandomizeDrawer();
    }

    void PuzzleFinished()
    {
        Debug.Log("Puzzle Finished! All candles are lit!");
        // Here you can trigger the Eye or next event
    }
}