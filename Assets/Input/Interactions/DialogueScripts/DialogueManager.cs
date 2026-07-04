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

    [Header("DEATHSCREEN")]
    public TextMeshProUGUI tipsText;

    private DialogueData.DialogueLine[] currentLines;
    private int index;
    private bool isPlayerFrozen = false;

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
        AutoBindReferences();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        player = FindFirstObjectByType<PlayerReferences>();
    }

    void AutoBindReferences()
    {
        if (dialoguePanel == null)
        {
            var obj = GameObject.Find("DIalogue_Panel");
            if (obj != null) dialoguePanel = obj;
        }

        if (dialogueText == null)
            dialogueText = FindByName<TextMeshProUGUI>("Dialogue_Text");

        if (nameText == null)
            nameText = FindByName<TextMeshProUGUI>("Name_Text");

        if (expressionImage == null)
            expressionImage = FindByName<Image>("ExpressionImage");

        if (tipsText == null)
            tipsText = FindByName<TextMeshProUGUI>("TipsText");
    }

    T FindByName<T>(string objectName) where T : Component
    {
        var all = FindObjectsByType<T>(FindObjectsSortMode.None);

        foreach (var item in all)
        {
            if (item.gameObject.name == objectName)
                return item;
        }

        return null;
    }

    void Update()
    {
        if (dialoguePanel == null)
            return;

        if (!dialoguePanel.activeSelf)
            return;

        if (!currentDialogueIsUnskippable)
        {
            bool advanceInputPressed = false;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                advanceInputPressed = true;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                advanceInputPressed = true;

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

    public void ShowTip(string tipID)
    {
        if (TipsDatabase.Instance == null)
        {
            Debug.LogWarning("TipsDatabase not found in scene.");
            return;
        }

        TipData tip = TipsDatabase.Instance.GetTip(tipID);

        if (tip != null && tipsText != null)
        {
            tipsText.text = tip.tipText;
        }
    }

    public void ShowRandomTip()
    {
        if (TipsDatabase.Instance == null)
        {
            Debug.LogWarning("TipsDatabase not found in scene.");
            return;
        }

        TipData tip = TipsDatabase.Instance.GetRandomTip();

        if (tip != null && tipsText != null)
        {
            tipsText.text = tip.tipText;
        }
    }
    public void SetTipText(TipData tip)
    {
        if (tipsText == null)
            tipsText = FindByName<TextMeshProUGUI>("TipsText");

        if (tipsText == null || tip == null)
            return;

        if (!tipsText.gameObject.activeSelf)
            tipsText.gameObject.SetActive(true);

        tipsText.text = tip.tipText;
        tipsText.color = tip.textColor;
    }
    private void OnEnable()
    {
        AutoBindReferences();
    }
}