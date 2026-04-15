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

    [Header("Drop")]
    public GameObject worldPrefab;

    [Header("Library Level 2 Data")]
    // 2. Add this field. By default, it's None, so it won't affect non-book items.
    public LibraryBookType bookType = LibraryBookType.None;
}