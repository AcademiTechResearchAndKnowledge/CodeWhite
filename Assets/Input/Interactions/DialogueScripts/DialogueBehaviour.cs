using UnityEngine;
using UnityEngine.Playables;

public class DialogueBehaviour : PlayableBehaviour
{
    public string dialogueID;
    public bool isUnskippable;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;

        if (!string.IsNullOrEmpty(dialogueID))
        {
            var dialogue = DialogueDatabase.Instance.GetDialogue(dialogueID);

            if (dialogue != null)
            {
                DialogueManager.Instance.StartDialogue(dialogue, isUnskippable);
            }
            else
            {
                Debug.LogWarning($"Timeline Dialogue Error: '{dialogueID}' not found in database.");
            }
        }
    }
}