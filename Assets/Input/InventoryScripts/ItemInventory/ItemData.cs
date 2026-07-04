using UnityEngine;

public enum StatType
{
    None,
    ANX,
    STA,
    SPD
}

public enum ItemEffectType
{
    None,
    Add,
    Subtract
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 5;
    [TextArea] public string description;

    [Header("Use Test")]
    [TextArea] public string useDebugMessage = "Used item.";
    public bool consumable = true;

    [Header("Item Effect")]
    public bool affectsStats = false;
    public StatType statType = StatType.None;
    public ItemEffectType effectType = ItemEffectType.None;
    public float statAmount = 0f;

    [Header("Drop")]
    public GameObject worldPrefab;

    [Header("Audio")]
    public AudioClip useSound;
}