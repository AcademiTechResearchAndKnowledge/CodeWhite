using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

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

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip noLighterSFX;
    public AudioClip puzzleFinishedSFX;

    [Header("Events")]
    public UnityEvent onPuzzleComplete;

    public enum LighterState { Hidden, Spawned, Held }
    public LighterState currentLighterState = LighterState.Hidden;

    private List<PuzzleDrawer> allDrawers = new List<PuzzleDrawer>();
    private List<PuzzleDrawer> unsearchedDrawers = new List<PuzzleDrawer>();

    private GameObject currentActiveEntity;

    private Stack<CandleInteract> litCandlesStack = new Stack<CandleInteract>();

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

    public void CandleLit(CandleInteract newlyLitCandle)
    {
        if (currentLighterState != LighterState.Held)
        {
            Debug.Log("Cannot light candle: Player does not have a lighter!");

            if (audioSource != null && noLighterSFX != null)
            {
                audioSource.PlayOneShot(noLighterSFX);
            }

            return;
        }

        candlesLit++;
        litCandlesStack.Push(newlyLitCandle);

        Debug.Log("Candles lit: " + candlesLit);

        currentLighterState = LighterState.Hidden;
        ResetAllDrawers();

        if (candlesLit >= candlesToFinish)
        {
            PuzzleFinished();
        }
        else
        {
            if (currentActiveEntity == null && entity_1 != null && entity_1_spawn != null)
            {
                currentActiveEntity = Instantiate(entity_1, entity_1_spawn.position, entity_1_spawn.rotation);
                Debug.Log("The entity has spawned!");
            }
        }
    }

    public void BlowOutCandle()
    {
        Debug.Log("Entity caught the player!");

        if (candlesLit > 0)
        {
            candlesLit--;
            Debug.Log("A candle was blown out. Candles lit: " + candlesLit);

            if (litCandlesStack.Count > 0)
            {
                CandleInteract candleToBlowOut = litCandlesStack.Pop();
                candleToBlowOut.Extinguish();
            }
        }
        currentLighterState = LighterState.Hidden;
        ResetAllDrawers();
    }

    private void ResetAllDrawers()
    {
        unsearchedDrawers.Clear();
        foreach (PuzzleDrawer drawer in allDrawers)
        {
            drawer.ResetSearchState();
            unsearchedDrawers.Add(drawer);
        }
        Debug.Log("All drawers reset! The pool is full again.");
    }

    void PuzzleFinished()
    {
        Debug.Log("Puzzle Finished! All candles are lit!");

        if (currentActiveEntity != null)
        {
            EntityDespawner despawner = currentActiveEntity.GetComponent<EntityDespawner>();
            if (despawner != null)
            {
                despawner.DespawnWithParticles();
            }
            else
            {
                Destroy(currentActiveEntity);
            }
        }

        if (audioSource != null && puzzleFinishedSFX != null)
        {
            audioSource.PlayOneShot(puzzleFinishedSFX);
        }

        onPuzzleComplete?.Invoke();
    }
}