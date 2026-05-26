using UnityEngine;

[CreateAssetMenu(fileName = "NewTip", menuName = "Tips/Tip Data")]
public class TipData : ScriptableObject
{
    public string tipID;

    [TextArea(2, 5)]
    public string tipText;

    [Header("Style")]
    public Color textColor = Color.white;
}