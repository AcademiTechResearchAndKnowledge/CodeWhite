using UnityEngine;

// FOR EMPTY OBJECT THAT MANAGES THE PUZZLE LOGIC
public class SwitchPuzzleManager : MonoBehaviour
{
    public static SwitchPuzzleManager Instance;

    [HideInInspector] public int correctButtonIndex = -1;
    [HideInInspector] public int completedCount = 0;
    [HideInInspector] public bool puzzleComplete = false;

    public int requiredCount = 5;

    void Awake()
    {
        Instance = this;
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
        int previous = correctButtonIndex;
        RandomizeCorrectButton(previous);
    }
}