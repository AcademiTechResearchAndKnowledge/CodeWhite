using UnityEngine;
using System.Collections.Generic;

public class DialogueDatabase : MonoBehaviour
{
    public static DialogueDatabase Instance;

    public DialogueData[] dialogues;

    private Dictionary<string, DialogueData> dialogueDict;

    private void Awake()
    {
        Instance = this;

        dialogueDict = new Dictionary<string, DialogueData>();

        foreach (var dialogue in dialogues)
        {
            dialogueDict.Add(dialogue.dialogueID, dialogue);
        }
    }

    public DialogueData GetDialogue(string id)
    {
        if (dialogueDict.ContainsKey(id))
            return dialogueDict[id];

        Debug.LogWarning("Dialogue ID not found: " + id);
        return null;
    }
}