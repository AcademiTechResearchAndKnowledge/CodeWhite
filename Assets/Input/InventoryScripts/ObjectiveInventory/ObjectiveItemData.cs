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

    [Header("Inventory Visuals")]
    [Tooltip("Add your 4 colored icons here (0=Red, 1=Blue, 2=Green, 3=Orange)")]
    public Sprite[] bookIcons;

    [Header("Item Usage")]
    public bool consumable = true;
    public bool spawnsPortal = false;

    [Header("Drop")]
    public GameObject worldPrefab;

    [Header("Library Level 2 Data")]
    public LibraryBookType bookType = LibraryBookType.None;

    [Header("Dynamic Runtime Data (Ignore in Inspector)")]
    public int visualIndex = -1; // 0=Red, 1=Blue, 2=Green, 3=Orange
}