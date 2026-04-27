using UnityEngine;
using TMPro;

// FOR LAPTOP'S PARENT OBJECT THAT HANDLES UI AND LOGIC
public class LaptopManager : MonoBehaviour, IZoomInteractable
{
    public static LaptopManager Instance;

    [Header("World Space Canvas on the laptop")]
    public GameObject laptopCanvas;
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public TMP_Text progressText;

    [Header("3D Answer objects on the laptop")]
    public LaptopAnswerButton[] answerButtons;
    public TMP_Text[] answerLabels;

    private bool laptopOpen = false;
    private bool answersUnlocked = false;
    private string pendingHint = "";

    public bool IsInteracting { get; set; }

    void Awake()
    {
        Instance = this;

        if (laptopCanvas != null)
            laptopCanvas.SetActive(false);

        SetupAnswerLabels();
        SetAnswerButtonsInteractable(false);
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


    public void StartInteraction()
    {
        IsInteracting = true;

        laptopOpen = true;

        if (laptopCanvas != null)
            laptopCanvas.SetActive(true);

        RefreshUI();
    }


    public void StopInteraction()
    {
        IsInteracting = false;

        laptopOpen = false;

        if (laptopCanvas != null)
            laptopCanvas.SetActive(false);
    }

    void RefreshUI()
    {
        if (SwitchPuzzleManager.Instance.puzzleComplete) return;

        questionText.text = "Which button lights up the bulb?";
        UpdateProgress();

        feedbackText.text = pendingHint != "" ? pendingHint : "";
    }

    void UpdateProgress()
    {
        if (progressText != null)
        {
            progressText.text = "Round " +
                (SwitchPuzzleManager.Instance.completedCount + 1) +
                " / " + SwitchPuzzleManager.Instance.requiredCount;
        }
    }

    public void UnlockAnswers()
    {
        answersUnlocked = true;
        SetAnswerButtonsInteractable(true);
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
            pendingHint = "";
            SwitchPuzzleManager.Instance.RegisterCorrectAnswer();
        }
        else
        {
            feedbackText.text = "That's wrong — try again!";
            pendingHint = "";
            SwitchPuzzleManager.Instance.RegisterWrongAnswer();
            ButtonController.Instance.UnlockButtons();
        }
    }

    void SetAnswerButtonsInteractable(bool state)
    {
        if (answerButtons == null) return;

        foreach (var btn in answerButtons)
        {
            if (btn != null)
                btn.enabled = state;
        }
    }

    public void ResetQuestion()
    {
        feedbackText.text = "";
        pendingHint = "";
        questionText.text = "Which button lights up the bulb?";
        UpdateProgress();
    }

    public void ShowHint(string message)
    {
        pendingHint = message;

        if (laptopOpen)
            feedbackText.text = pendingHint;
    }

    public void ShowObjectiveComplete()
    {
        laptopOpen = true;

        if (laptopCanvas != null)
            laptopCanvas.SetActive(true);

        questionText.text = "Objective complete!";
        feedbackText.text = "You solved all five rounds. Well done.";

        if (progressText != null)
            progressText.text = "Done!";

        SetAnswerButtonsInteractable(false);
    }
}