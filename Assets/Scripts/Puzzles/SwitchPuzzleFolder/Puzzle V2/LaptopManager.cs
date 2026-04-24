using UnityEngine;
using TMPro;

// FOR LAPTOP'S PARENT OBJECT THAT HANDLES THE UI AND LOGIC OF THE QUESTION AND ANSWERS
public class LaptopManager : MonoBehaviour
{
    public static LaptopManager Instance;

    [Header("World Space Canvas on the laptop")]
    public GameObject laptopCanvas;
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public TMP_Text progressText;

    [Header("3D Answer objects on the laptop (LaptopAnswerButton components)")]
    public LaptopAnswerButton[] answerButtons;
    public TMP_Text[] answerLabels;

    private bool laptopOpen = false;
    private bool answersUnlocked = false;
    private string pendingHint = "";

    void Awake()
    {
        Instance = this;
        laptopCanvas.SetActive(false);
        SetupAnswerLabels();
        SetAnswerButtonsInteractable(false);
    }

    void SetupAnswerLabels()
    {
        if (answerLabels == null) return;
        for (int i = 0; i < answerLabels.Length; i++)
            if (answerLabels[i] != null)
                answerLabels[i].text = "Button " + (i + 1);
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
        UpdateProgress();

        // Show bulb hint if player already checked it, otherwise show nothing
        feedbackText.text = pendingHint != "" ? pendingHint : "";
    }

    void UpdateProgress()
    {
        if (progressText != null)
            progressText.text = "Round " + (SwitchPuzzleManager.Instance.completedCount + 1)
                                + " / " + SwitchPuzzleManager.Instance.requiredCount;
    }

    public void UnlockAnswers()
    {
        answersUnlocked = true;
        SetAnswerButtonsInteractable(true);
        // Do NOT clear pendingHint here — player may not have checked bulb yet
        // pendingHint is only cleared when a new round fully begins
    }

    void LockAnswers()
    {
        answersUnlocked = false;
        SetAnswerButtonsInteractable(false);
    }

    public void OnAnswerSelected(int buttonIndex)
    {
        if (SwitchPuzzleManager.Instance.puzzleComplete) return;
        if (!laptopOpen) return;
        if (!answersUnlocked) return;

        LockAnswers();

        if (SwitchPuzzleManager.Instance.CheckAnswer(buttonIndex))
        {
            feedbackText.text = "You got the right answer!";
            pendingHint = "";           // clear hint — new round starting
            SwitchPuzzleManager.Instance.RegisterCorrectAnswer();
        }
        else
        {
            feedbackText.text = "That's wrong — try again!";
            pendingHint = "";           // clear hint — player must press buttons again
            SwitchPuzzleManager.Instance.RegisterWrongAnswer();
            ButtonController.Instance.UnlockButtons();
        }
    }

    void SetAnswerButtonsInteractable(bool state)
    {
        if (answerButtons == null) return;
        foreach (var btn in answerButtons)
            if (btn != null) btn.enabled = state;
    }

    public void ResetQuestion()
    {
        feedbackText.text = "";
        pendingHint = "";
        questionText.text = "Which button lights up the bulb?";
        UpdateProgress();
    }

    // Bulb calls this — only updates text, never opens the laptop
    public void ShowHint(string message)
    {
        pendingHint = message;
        if (laptopOpen)
            feedbackText.text = pendingHint;
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