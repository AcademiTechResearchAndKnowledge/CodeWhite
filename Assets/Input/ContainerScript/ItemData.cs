using UnityEngine;

public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }
public enum ItemType { Weapon, Armor, Consumable, Quest, Currency, Accessory }

[CreateAssetMenu(fileName = "NewItem", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public string description;
    public Sprite sprite;
}