using UnityEngine;

public enum LibraryBookType
{
    None,
    Signed,
    Unsigned,
    Forged
}

[CreateAssetMenu(fileName = "New Objective Item", menuName = "Inventory/Objective Item Data")]
public class ObjectiveItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 5;
    [TextArea] public string description;

    [Header("Item Usage")]
    public bool consumable = true;     // Add this!
    public bool spawnsPortal = false;  // Add this!

    [Header("Drop")]
    public GameObject worldPrefab;

    [Header("Library Level 2 Data")]
    public LibraryBookType bookType = LibraryBookType.None;
}