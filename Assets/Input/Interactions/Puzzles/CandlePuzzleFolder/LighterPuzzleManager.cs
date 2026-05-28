using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using TMPro;

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

    [Header("Entity Spawn Effects")] // <-- NEW SECTION
    [Tooltip("The sound to play when the entity is summoned into the room.")]
    public AudioClip entitySpawnSFX;
    [Tooltip("Volume for the spawn sound.")]
    [Range(0f, 1f)] public float spawnVolume = 1.0f;

    [Header("Entity Despawn Effects")]
    [Tooltip("The particle effect to spawn when the entity disappears.")]
    public GameObject despawnParticlePrefab;
    [Tooltip("The sound to play when the entity despawns.")]
    public AudioClip despawnSound;
    [Tooltip("Volume for the despawn sound.")]
    [Range(0f, 1f)] public float despawnVolume = 1.0f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip noLighterSFX;
    public AudioClip puzzleFinishedSFX;

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
            PlayNoLighterError("You need a lighter to do this!");
            return;
        }

        candlesLit++;
        litCandlesStack.Push(newlyLitCandle);

        currentLighterState = LighterState.Hidden;
        ResetAllDrawers();

        if (candlesLit >= candlesToFinish)
        {
            PuzzleFinished();
        }
        else
        {
            int candlesRemaining = candlesToFinish - candlesLit;

            if (currentActiveEntity == null && entity_1 != null && entity_1_spawn != null)
            {
                currentActiveEntity = Instantiate(entity_1, entity_1_spawn.position, entity_1_spawn.rotation);

                // --- NEW: PLAY SPAWN AUDIO ---
                if (entitySpawnSFX != null && audioSource != null)
                {
                    audioSource.PlayOneShot(entitySpawnSFX, spawnVolume);
                }

                ShowDialogue($"Something is here... {candlesRemaining} candles remain)");
            }
            else
            {
                ShowDialogue($"{candlesRemaining} more candles left");
            }
        }
    }

    public void BlowOutCandle()
    {
        if (candlesLit > 0)
        {
            candlesLit--;
            if (litCandlesStack.Count > 0)
            {
                CandleInteract candleToBlowOut = litCandlesStack.Pop();
                candleToBlowOut.Extinguish();
            }

            ShowDialogue("The darkness grows... One of the candles went out...");
        }
        else
        {
            ShowDialogue("The entity caught you in the dark!");
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
    }

    void PuzzleFinished()
    {
        ShowDialogue("All Candles are Lit! Opening portal...");

        if (currentActiveEntity != null)
        {
            if (despawnParticlePrefab != null)
            {
                Instantiate(despawnParticlePrefab, currentActiveEntity.transform.position, currentActiveEntity.transform.rotation);
            }

            if (despawnSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(despawnSound, despawnVolume);
            }

            Destroy(currentActiveEntity);
            currentActiveEntity = null;
        }

        if (audioSource != null && puzzleFinishedSFX != null)
        {
            audioSource.PlayOneShot(puzzleFinishedSFX);
        }

        onPuzzleComplete?.Invoke();
    }

    public void PlayNoLighterError(string message = "You need a lighter to light this!")
    {
        if (audioSource != null && noLighterSFX != null)
        {
            audioSource.PlayOneShot(noLighterSFX);
        }

        ShowDialogue(message);
    }

    private void ShowDialogue(string message)
    {
        if (hintText != null)
        {
            hintText.text = message;
            StopAllCoroutines();
            StartCoroutine(ClearHintText());
        }
    }

    private IEnumerator ClearHintText()
    {
        yield return new WaitForSeconds(hintDisplayTime);
        if (hintText != null)
        {
            hintText.text = "";
        }
    }
}