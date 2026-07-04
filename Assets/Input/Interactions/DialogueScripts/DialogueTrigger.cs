using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public string dialogueID;

    public void TriggerDialogue()
    {
        var dialogue = DialogueDatabase.Instance.GetDialogue(dialogueID);
        DialogueManager.Instance.StartDialogue(dialogue);
    }
}