using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class StageGroup
{
    [Tooltip("The boss stage number for this group. Example: 10, 20, 30.")]
    public int bossStageNumber;

    [Tooltip("Normal scenes used before the boss stage. Example: for boss stage 10, these are used for stages 1-9.")]
    public List<string> normalScenes = new();

    [Tooltip("Fixed boss scene that will always load on this exact stage number.")]
    public string bossScene;
}

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("Tutorial")]
    public bool playTutorialFirstRun = true;
    public string tutorialSceneName = "T_Intro";

    [Header("Stage Groups")]
    [Tooltip("Example: bossStageNumber 10 = stages 1-9 random from normalScenes, then stage 10 loads bossScene.")]
    public List<StageGroup> stageGroups = new();

    [Header("Anti-repeat")]
    public bool avoidImmediateRepeat = true;

    // Run State
    public int StageIndex { get; private set; } = 0; // 0 before first stage
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
    /// Start a new run from menu or new game button.
    /// </summary>
    public void StartNewRun()
    {
        StageIndex = 0;
        TutorialDone = false;
        lastLoadedScene = SceneManager.GetActiveScene().name;

        LoadNextStage();
    }

    /// <summary>
    /// Loads the next stage based on your 10-stage group logic.
    /// </summary>
    public void LoadNextStage()
    {
        // Play tutorial once first
        if (playTutorialFirstRun && !TutorialDone && !string.IsNullOrEmpty(tutorialSceneName))
        {
            TutorialDone = true;
            LoadSceneSafe(tutorialSceneName);
            return;
        }

        // Advance stage
        StageIndex++;

        string sceneToLoad = GetSceneForCurrentStage();

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError($"RunManager: No scene found for Stage {StageIndex}. Check your Stage Groups in the Inspector.");
            return;
        }

        LoadSceneSafe(sceneToLoad);
    }

    private string GetSceneForCurrentStage()
    {
        StageGroup group = GetGroupForStage(StageIndex);

        if (group == null)
        {
            Debug.LogError($"RunManager: No StageGroup matches Stage {StageIndex}.");
            return "";
        }

        // Boss stage (10, 20, 30, etc.)
        if (StageIndex == group.bossStageNumber)
        {
            if (string.IsNullOrEmpty(group.bossScene))
            {
                Debug.LogError($"RunManager: Boss scene is missing for boss stage {group.bossStageNumber}.");
                return "";
            }

            return group.bossScene;
        }

        // Normal stage inside that block
        return PickFromList(group.normalScenes);
    }

    private StageGroup GetGroupForStage(int stage)
    {
        if (stageGroups == null || stageGroups.Count == 0)
            return null;

        // Example:
        // stage 1-10  -> group with bossStageNumber = 10
        // stage 11-20 -> group with bossStageNumber = 20
        // stage 21-30 -> group with bossStageNumber = 30

        foreach (var group in stageGroups)
        {
            if (group == null) continue;

            int startStage = group.bossStageNumber - 9;
            int endStage = group.bossStageNumber;

            if (stage >= startStage && stage <= endStage)
                return group;
        }

        return null;
    }

    private void LoadSceneSafe(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("RunManager: Tried to load an empty scene name.");
            return;
        }

        // Avoid immediate repeat only for normal scenes
        if (avoidImmediateRepeat && sceneName == SceneManager.GetActiveScene().name)
        {
            string alt = TryPickDifferent(sceneName);
            if (!string.IsNullOrEmpty(alt))
                sceneName = alt;
        }

        lastLoadedScene = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    private string PickFromList(List<string> list)
    {
        if (list == null || list.Count == 0)
            return "";

        List<string> candidates = new();

        foreach (var s in list)
        {
            if (string.IsNullOrEmpty(s)) continue;
            if (!string.IsNullOrEmpty(tutorialSceneName) && s == tutorialSceneName) continue;
            candidates.Add(s);
        }

        if (candidates.Count == 0)
            return "";

        if (avoidImmediateRepeat)
        {
            string current = SceneManager.GetActiveScene().name;

            List<string> filtered = new();
            foreach (var s in candidates)
            {
                if (s != current)
                    filtered.Add(s);
            }

            if (filtered.Count > 0)
                return filtered[Random.Range(0, filtered.Count)];
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private string TryPickDifferent(string sameScene)
    {
        StageGroup group = GetGroupForStage(StageIndex);
        if (group == null || group.normalScenes == null || group.normalScenes.Count <= 1)
            return "";

        List<string> valid = new();

        foreach (var s in group.normalScenes)
        {
            if (string.IsNullOrEmpty(s)) continue;
            if (s == sameScene) continue;
            if (!string.IsNullOrEmpty(tutorialSceneName) && s == tutorialSceneName) continue;
            valid.Add(s);
        }

        if (valid.Count == 0)
            return "";

        return valid[Random.Range(0, valid.Count)];
    }
}