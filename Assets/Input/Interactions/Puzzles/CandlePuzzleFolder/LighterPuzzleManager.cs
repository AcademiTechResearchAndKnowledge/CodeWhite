using UnityEngine;
using System.Collections.Generic;

public class LighterPuzzleManager : MonoBehaviour
{
    public static LighterPuzzleManager instance;

    [Header("Puzzle Settings")]
    public int candlesLit = 0;
    public int candlesToFinish = 3;

    [Range(0f, 1f)] public float baseSpawnChance = 0.15f;

    [Header("Prefabs & Entities")]
    public GameObject lighterPrefab;
    public GameObject entity_1;
    public Transform entity_1_spawn;

    public enum LighterState { Hidden, Spawned, Held }
    public LighterState currentLighterState = LighterState.Hidden;

    // We now track TWO lists. One for all drawers, one for the active unsearched pool
    private List<PuzzleDrawer> allDrawers = new List<PuzzleDrawer>();
    private List<PuzzleDrawer> unsearchedDrawers = new List<PuzzleDrawer>();

    void Awake()
    {
        instance = this;
    }

    public void RegisterDrawer(PuzzleDrawer drawer)
    {
        if (!allDrawers.Contains(drawer))
        {
            allDrawers.Add(drawer);
            unsearchedDrawers.Add(drawer);
        }
    }

    public bool TrySpawnLighter(PuzzleDrawer drawer)
    {
        unsearchedDrawers.Remove(drawer);

        if (currentLighterState != LighterState.Hidden)
            return false;

        bool shouldSpawn = false;

        if (unsearchedDrawers.Count == 0)
        {
            shouldSpawn = true;
            Debug.Log("Last drawer! Guaranteed lighter spawn.");
        }
        else
        {
            shouldSpawn = Random.value <= baseSpawnChance;
        }

        if (shouldSpawn)
        {
            currentLighterState = LighterState.Spawned;
            return true;
        }

        return false;
    }

    public void OnLighterPickedUp()
    {
        currentLighterState = LighterState.Held;
    }

    public void CandleLit()
    {
        candlesLit++;
        Debug.Log("Candles lit: " + candlesLit);

        currentLighterState = LighterState.Hidden;

        // THE FIX: Reset all drawers so the player can search them again for the next lighter!
        ResetAllDrawers();

        if (candlesLit == 1 && entity_1 != null && entity_1_spawn != null)
        {
            Instantiate(entity_1, entity_1_spawn.position, entity_1_spawn.rotation);
        }

        if (candlesLit >= candlesToFinish)
        {
            PuzzleFinished();
        }
    }

    private void ResetAllDrawers()
    {
        unsearchedDrawers.Clear();
        foreach (PuzzleDrawer drawer in allDrawers)
        {
            drawer.ResetSearchState(); // We will add this method to the drawer script next
            unsearchedDrawers.Add(drawer);
        }
        Debug.Log("All drawers reset! The pool is full again.");
    }

    void PuzzleFinished()
    {
        Debug.Log("Puzzle Finished! All candles are lit!");
    }
}