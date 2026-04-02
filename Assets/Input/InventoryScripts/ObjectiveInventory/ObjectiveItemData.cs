using UnityEngine;

[CreateAssetMenu(fileName = "New Objective Item", menuName = "Inventory/Objective Item Data")]
public class ObjectiveItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 5; // Added stack limit
    [TextArea] public string description;

    [Header("Drop")]
    public GameObject worldPrefab;
}