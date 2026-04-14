using UnityEngine;

public class SwitchPuzzleManager : MonoBehaviour
{
    public static SwitchPuzzleManager Instance;

    [HideInInspector] public int correctButtonIndex = 0;
    [HideInInspector] public int completedCount = 0;
    [HideInInspector] public bool puzzleComplete = false;

    public int requiredCount = 5;

    void Awake()
    {
        Instance = this;
        RandomizeCorrectButton();
    }

    public void RandomizeCorrectButton()
    {
        correctButtonIndex = Random.Range(0, 3);
        Debug.Log("[Puzzle] Correct button is now index: " + correctButtonIndex);
    }

    public bool CheckAnswer(int buttonIndex)
    {
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
            RandomizeCorrectButton();
            ButtonController.Instance.OnCorrectAnswerGiven();
            LaptopManager.Instance.ResetQuestion();
        }
    }
}