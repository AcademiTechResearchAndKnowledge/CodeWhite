using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string dialogueID;

    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;

        [TextArea]
        public string text;

        public Color textColor = Color.white;
        public Color nameColor = Color.white;
    }

    public DialogueLine[] lines;
    public bool freezePlayer = true;
    public bool useAutoClose = false;
    public float autoCloseTime = 2f;
}