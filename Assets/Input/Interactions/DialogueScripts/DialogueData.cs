using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string dialogueID;
    [TextArea]
    public string[] lines;
    public bool freezePlayer = true;
    public bool useAutoClose = false;
    public float autoCloseTime = 2f; // seconds
}