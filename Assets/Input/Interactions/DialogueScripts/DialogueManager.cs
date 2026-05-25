using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;
    public Image expressionImage;

    private DialogueData.DialogueLine[] currentLines;
    private int index;
    private bool isPlayerFrozen = false;

    // CHANGED: Now using PlayerReferences instead of PlayerMovement
    private PlayerReferences player;

    private bool useAutoAdvance = false;
    private float autoAdvanceTime = 5f;
    private float autoAdvanceTimer = 0f;

    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private bool currentDialogueIsUnskippable = false;
    private Action onDialogueComplete;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);

        player = FindFirstObjectByType<PlayerReferences>();
    }

    void Update()
    {
        if (!dialoguePanel.activeSelf) return;

        if (!currentDialogueIsUnskippable)
        {
            bool advanceInputPressed = false;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                advanceInputPressed = true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                advanceInputPressed = true;
            }

            if (advanceInputPressed)
            {
                if (isTyping)
                    SkipTyping();
                else
                    NextLine();
            }
        }

        if (useAutoAdvance && !isTyping)
        {
            autoAdvanceTimer -= Time.unscaledDeltaTime;
            if (autoAdvanceTimer <= 0f)
                NextLine();
        }
    }

    public void StartDialogue(DialogueData data, bool unskippable = false, Action onComplete = null)
    {
        if (data == null) return;

        currentDialogueIsUnskippable = unskippable;
        onDialogueComplete = onComplete;

        currentLines = data.lines;
        index = 0;
        dialoguePanel.SetActive(true);
        ShowLine();

        // CHANGED: Freeze player movement AND looking if PlayerReferences is found
        if (data.freezePlayer && player != null)
        {
            if (player.movementScript != null) player.movementScript.enabled = false;
            if (player.playerLook != null) player.playerLook.canLook = false;
            isPlayerFrozen = true;
        }

        useAutoAdvance = data.useAutoClose;
        autoAdvanceTime = data.autoCloseTime;

        if (useAutoAdvance)
            autoAdvanceTimer = autoAdvanceTime;
    }

    void ShowLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        var line = currentLines[index];

        if (string.IsNullOrEmpty(line.speakerName))
            nameText.gameObject.SetActive(false);
        else
        {
            nameText.gameObject.SetActive(true);
            nameText.text = line.speakerName;
            nameText.color = line.nameColor;
        }

        if (expressionImage != null)
        {
            if (line.expression != null)
            {
                expressionImage.sprite = line.expression;
                expressionImage.gameObject.SetActive(true);
            }
            else
            {
                expressionImage.gameObject.SetActive(false);
            }
        }

        dialogueText.color = line.textColor;
        typingCoroutine = StartCoroutine(TypeText(line.text));

        if (useAutoAdvance)
            autoAdvanceTimer = autoAdvanceTime;
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(0.02f);
        }

        isTyping = false;
    }

    void SkipTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.text = currentLines[index].text;
        isTyping = false;
    }

    void NextLine()
    {
        index++;
        if (index >= currentLines.Length)
            EndDialogue();
        else
            ShowLine();
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // CHANGED: Unfreeze player movement AND looking
        if (isPlayerFrozen && player != null)
        {
            if (player.movementScript != null) player.movementScript.enabled = true;
            if (player.playerLook != null) player.playerLook.canLook = true;
            isPlayerFrozen = false;
        }

        useAutoAdvance = false;

        Action tempAction = onDialogueComplete;
        onDialogueComplete = null;
        tempAction?.Invoke();
    }
}