using UnityEngine;
using TMPro;

public class LaptopManager : MonoBehaviour
{
    public static LaptopManager Instance;

    [Header("World Space Canvas on the laptop")]
    public GameObject laptopCanvas;
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public TMP_Text progressText;

    [Header("3D Answer objects on the laptop (LaptopAnswerButton components)")]
    public LaptopAnswerButton[] answerButtons;  // drag the 3 physical answer GameObjects here
    public TMP_Text[] answerLabels;             // TMP labels on those objects (optional)

    private bool laptopOpen = false;

    void Awake()
    {
        Instance = this;
        laptopCanvas.SetActive(false);
        SetupAnswerLabels();
    }

    void SetupAnswerLabels()
    {
        if (answerLabels == null) return;
        for (int i = 0; i < answerLabels.Length; i++)
        {
            if (answerLabels[i] != null)
                answerLabels[i].text = "Button " + (i + 1);
        }
    }

    public void ToggleLaptop()
    {
        laptopOpen = !laptopOpen;
        laptopCanvas.SetActive(laptopOpen);

        if (laptopOpen) RefreshUI();
    }

    void RefreshUI()
    {
        if (SwitchPuzzleManager.Instance.puzzleComplete) return;
        questionText.text = "Which button lights up the bulb?";
        feedbackText.text = "";
        UpdateProgress();
        SetAnswerButtonsInteractable(true);
    }

    void UpdateProgress()
    {
        if (progressText != null)
            progressText.text = "Round " + (SwitchPuzzleManager.Instance.completedCount + 1)
                                + " / " + SwitchPuzzleManager.Instance.requiredCount;
    }

    // Called by LaptopAnswerButton when player interacts with an answer
    public void OnAnswerSelected(int buttonIndex)
    {
        if (SwitchPuzzleManager.Instance.puzzleComplete) return;
        if (!laptopOpen) return;

        if (SwitchPuzzleManager.Instance.CheckAnswer(buttonIndex))
        {
            feedbackText.text = "You got the right answer!";
            SetAnswerButtonsInteractable(false);
            SwitchPuzzleManager.Instance.RegisterCorrectAnswer();
            // UnlockButtons is handled inside OnCorrectAnswerGiven via SwitchPuzzleManager
        }
        else
        {
            feedbackText.text = "That's wrong — try again!";
            // Only unlock buttons on a wrong answer so player can try again
            ButtonController.Instance.UnlockButtons();
        }
    }

    void SetAnswerButtonsInteractable(bool state)
    {
        if (answerButtons == null) return;
        foreach (var btn in answerButtons)
        {
            if (btn != null) btn.enabled = state;
        }
    }

    public void ResetQuestion()
    {
        feedbackText.text = "";
        questionText.text = "Which button lights up the bulb?";
        UpdateProgress();
        SetAnswerButtonsInteractable(true);
    }

    public void ShowHint(string message)
    {
        if (!laptopOpen)
        {
            laptopOpen = true;
            laptopCanvas.SetActive(true);
            RefreshUI();
        }
        feedbackText.text = message;
    }

    public void ShowObjectiveComplete()
    {
        laptopOpen = true;
        laptopCanvas.SetActive(true);
        questionText.text = "Objective complete!";
        feedbackText.text = "You solved all five rounds. Well done.";
        if (progressText != null) progressText.text = "Done!";
        SetAnswerButtonsInteractable(false);
    }
}