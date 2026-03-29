using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private string[] currentLines;
    private int index;
    private bool isPlayerFrozen = false;
    private PlayerMovement playerMovement;

    private bool useAutoAdvance = false;
    private float autoAdvanceTime = 5f;
    private float autoAdvanceTimer = 0f;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
        playerMovement = Object.FindFirstObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        // Manual advance (optinal to hahaha)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            NextLine();
        }

        // Auto advance
        if (useAutoAdvance)
        {
            autoAdvanceTimer -= Time.unscaledDeltaTime;
            if (autoAdvanceTimer <= 0f)
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(DialogueData data)
    {
        if (data == null) return;

        currentLines = data.lines;
        index = 0;
        dialoguePanel.SetActive(true);
        ShowLine();

        if (data.freezePlayer && playerMovement != null)
        {
            playerMovement.enabled = false;
            isPlayerFrozen = true;
        }

        useAutoAdvance = data.useAutoClose;
        autoAdvanceTime = data.autoCloseTime;

        if (useAutoAdvance)
        {
            autoAdvanceTimer = autoAdvanceTime;
        }
    }

    void ShowLine()
    {
        dialogueText.text = currentLines[index];
        if (useAutoAdvance)
        {
            autoAdvanceTimer = autoAdvanceTime; // reset timer on new line
        }
    }

    void NextLine()
    {
        index++;
        if (index >= currentLines.Length)
        {
            EndDialogue();
        }
        else
        {
            ShowLine();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        if (isPlayerFrozen && playerMovement != null)
        {
            playerMovement.enabled = true;
            isPlayerFrozen = false;
        }
        useAutoAdvance = false;
    }
}