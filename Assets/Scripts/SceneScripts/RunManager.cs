using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("Rules")]
    [Tooltip("Example: 3 means stages 3, 6, 9... are boss stages.")]
    [Min(1)]
    public int bossEveryXStages = 3;

    [Header("Tutorial")]
    public bool playTutorialFirstRun = true;
    public string tutorialSceneName = "T_Intro";

    [Header("Level Pools")]
    public List<string> easyLevels = new();
    public List<string> mediumLevels = new();
    public List<string> hardLevels = new();

    [Header("Boss Pool")]
    public List<string> bossLevels = new();

    [Header("Anti-repeat")]
    public bool avoidImmediateRepeat = true;

    // Run State
    public int StageIndex { get; private set; } = 0; // 0 before first stage; 1 = first stage; etc.
    public bool TutorialDone { get; private set; } = false;

    private string lastLoadedScene = "";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        lastLoadedScene = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// Start a new run from menu or a new game button.
    /// </summary>
    public void StartNewRun()
    {
        StageIndex = 0;
        TutorialDone = false;
        lastLoadedScene = SceneManager.GetActiveScene().name;

        LoadNextStage();
    }

    /// <summary>
    /// Loads the next stage based on rules (tutorial once, boss every X).
    /// Call this from your portal trigger.
    /// </summary>
    public void LoadNextStage()
    {
        // Tutorial only once (never included in random pools)
        if (playTutorialFirstRun && !TutorialDone && !string.IsNullOrEmpty(tutorialSceneName))
        {
            TutorialDone = true;
            LoadSceneSafe(tutorialSceneName);
            return;
        }

        // Move to the next stage number
        StageIndex++;

        bool isBossStage = (bossEveryXStages > 0) && (StageIndex % bossEveryXStages == 0);

        string sceneToLoad = isBossStage ? PickBossScene() : PickNormalScene();

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("RunManager: Could not pick a scene. Check your level lists (easy/medium/hard/boss).");
            return;
        }

        LoadSceneSafe(sceneToLoad);
    }

    private void LoadSceneSafe(string sceneName)
    {
        // Optional: avoid loading the exact same scene again immediately
        if (avoidImmediateRepeat && sceneName == SceneManager.GetActiveScene().name)
        {
            // try to pick a different one once
            string alt = TryPickDifferent(sceneName);
            if (!string.IsNullOrEmpty(alt)) sceneName = alt;
        }

        lastLoadedScene = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    private string PickNormalScene()
    {
        // Simple difficulty ramp (adjust however you want)
        // stages 10: easy, 20: medium, 20+: hard
        if (StageIndex <= 10) return PickFromList(easyLevels);
        if (StageIndex <= 20) return PickFromList(mediumLevels);
        return PickFromList(hardLevels);
    }

    private string PickBossScene()
    {
        return PickFromList(bossLevels);
    }

    private string PickFromList(List<string> list)
    {
        if (list == null || list.Count == 0) return "";

        // Filter out tutorial name if someone accidentally put it in a list
        List<string> candidates = new();
        foreach (var s in list)
        {
            if (string.IsNullOrEmpty(s)) continue;
            if (!string.IsNullOrEmpty(tutorialSceneName) && s == tutorialSceneName) continue;
            candidates.Add(s);
        }

        if (candidates.Count == 0) return "";

        if (avoidImmediateRepeat)
        {
            // Try a few times to avoid repeating current scene
            string current = SceneManager.GetActiveScene().name;
            for (int i = 0; i < 8; i++)
            {
                string pick = candidates[Random.Range(0, candidates.Count)];
                if (pick != current) return pick;
            }
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private string TryPickDifferent(string sameScene)
    {
        // Try to pick another scene from the correct pool (boss vs normal)
        bool isBossStage = (bossEveryXStages > 0) && (StageIndex % bossEveryXStages == 0);
        List<string> pool = isBossStage ? bossLevels : (StageIndex <= 2 ? easyLevels : (StageIndex <= 4 ? mediumLevels : hardLevels));

        if (pool == null || pool.Count <= 1) return "";

        for (int i = 0; i < 10; i++)
        {
            string pick = pool[Random.Range(0, pool.Count)];
            if (!string.IsNullOrEmpty(pick) && pick != sameScene && pick != tutorialSceneName)
                return pick;
        }

        return "";
    }
}