using UnityEngine;

public class SwitchPuzzleManager : MonoBehaviour
{
    public static SwitchPuzzleManager Instance;

    [Header("Puzzle Settings")]
    [HideInInspector] public int correctButtonIndex = -1;
    [HideInInspector] public int completedCount = 0;
    public bool puzzleComplete = false;
    public int requiredCount = 5;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    void Awake()
    {
        Instance = this;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        RandomizeCorrectButton(-1);
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
        if (correctButtonIndex == -1)
        {
            Debug.LogWarning("[Puzzle] CheckAnswer called before RandomizeCorrectButton ran.");
            return false;
        }
        return buttonIndex == correctButtonIndex;
    }

    public void RegisterCorrectAnswer()
    {
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
        if (audioSource != null && wrongSFX != null)
        {
            audioSource.PlayOneShot(wrongSFX);
        }

        int previous = correctButtonIndex;
        RandomizeCorrectButton(previous);
    }
}