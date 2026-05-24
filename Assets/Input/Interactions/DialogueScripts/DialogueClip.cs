using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class DialogueClip : PlayableAsset, ITimelineClipAsset
{
    public string dialogueID;

    [Tooltip("If checked, the player cannot skip or speed up this dialogue.")]
    public bool isUnskippable;

    public ClipCaps clipCaps => ClipCaps.None;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueBehaviour>.Create(graph);

        DialogueBehaviour behaviour = playable.GetBehaviour();
        behaviour.dialogueID = dialogueID;
        behaviour.isUnskippable = isUnskippable;

        return playable;
    }
}