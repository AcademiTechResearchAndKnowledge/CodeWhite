using UnityEngine;
using System.Collections; // NEW: Required for timers
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro; // NEW: Required for TextMeshPro

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

    // NEW: UI Settings
    [Header("UI Settings")]
    [Tooltip("Drag the TextMeshPro UI object from this scene's Hierarchy into this slot.")]
    public TextMeshProUGUI hintText;
    public float hintDisplayTime = 3f;

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

    // NEW: Clear the text when the scene starts
    void Start()
    {
        if (hintText != null)
        {
            hintText.text = "";
        }
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

            // Trigger the error text and sound
            PlayNoLighterError("You need a lighter to do this!");

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

    // NEW: Method to play the sound and show the hint text
    public void PlayNoLighterError(string message = "You need a lighter to light this!")
    {
        if (audioSource != null && noLighterSFX != null)
        {
            audioSource.PlayOneShot(noLighterSFX);
        }

        if (hintText != null)
        {
            hintText.text = message;
            StopAllCoroutines();
            StartCoroutine(ClearHintText());
        }
    }

    // NEW: Timer to clear the text
    private IEnumerator ClearHintText()
    {
        yield return new WaitForSeconds(hintDisplayTime);
        if (hintText != null)
        {
            hintText.text = "";
        }
    }
}