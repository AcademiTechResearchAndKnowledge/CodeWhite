using System.Collections;
using UnityEngine;

public class SwitchPuzzleManager : MonoBehaviour
{
    public static SwitchPuzzleManager Instance;

    [Header("Puzzle Settings")]
    [HideInInspector] public int correctButtonIndex = -1;
    [HideInInspector] public int completedCount = 0;
    public bool puzzleComplete = false;
    public int requiredCount = 5;

    [Header("Penalty Settings")]
    [Tooltip("Drag the specific Jumpscare Canvas PREFAB here.")]
    public JumpscareMechanic jumpscarePrefab;
    private int wrongCount = 0;
    private bool isPenaltyActive = false;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    private PlayerStats playerStats;
    private AnxietyHandler anxietyHandler;

    void Awake()
    {
        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        RandomizeCorrectButton(-1);
    }

    void Start()
    {
        GameObject actualPlayer = GameObject.FindGameObjectWithTag("Player");
        if (actualPlayer != null)
        {
            playerStats = actualPlayer.GetComponent<PlayerStats>();
            anxietyHandler = actualPlayer.GetComponent<AnxietyHandler>();
        }
    }

    public void RandomizeCorrectButton(int previousIndex)
    {
        int newIndex;
        do
        {
            newIndex = Random.Range(0, 3);
        }
        while (newIndex == previousIndex);

        correctButtonIndex = newIndex;
        Debug.Log("[Puzzle] Correct button is now index: " + correctButtonIndex);
    }

    public bool CheckAnswer(int buttonIndex)
    {
        if (correctButtonIndex == -1 || puzzleComplete || isPenaltyActive)
        {
            return false;
        }
        return buttonIndex == correctButtonIndex;
    }

    public void RegisterCorrectAnswer()
    {
        if (isPenaltyActive) return;

        if (audioSource != null && correctSFX != null)
        {
            audioSource.PlayOneShot(correctSFX);
        }

        completedCount++;

        if (completedCount >= requiredCount)
        {
            puzzleComplete = true;
            LaptopManager.Instance.ShowObjectiveComplete();
        }
        else
        {
            int previous = correctButtonIndex;
            RandomizeCorrectButton(previous);
            ButtonController.Instance.OnCorrectAnswerGiven();
            LaptopManager.Instance.ResetQuestion();
        }
    }

    public void RegisterWrongAnswer()
    {
        if (isPenaltyActive) return;

        if (audioSource != null && wrongSFX != null)
        {
            audioSource.PlayOneShot(wrongSFX);
        }

        StartCoroutine(WrongAnswerPenaltyRoutine());
    }

    private IEnumerator WrongAnswerPenaltyRoutine()
    {
        isPenaltyActive = true;
        wrongCount++;

        float waitTime = 1.0f;

        if (jumpscarePrefab != null)
        {
            JumpscareMechanic spawnedJumpscare = Instantiate(jumpscarePrefab);
            spawnedJumpscare.TriggerJumpscare();

            waitTime = spawnedJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(Mathf.Max(0f, waitTime));

        if (playerStats != null)
        {
            float anxietyToAdd = Mathf.Clamp(wrongCount * 20f, 0f, 100f);
            playerStats.AddStat(StatType.ANX, anxietyToAdd);

            if (anxietyHandler != null)
            {
                anxietyHandler.ResetSafeTimer();
            }
        }

        int previous = correctButtonIndex;
        RandomizeCorrectButton(previous);

        isPenaltyActive = false;
    }
}