using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int hotbarSize = 5;
    public List<InventorySlot> slots = new List<InventorySlot>();
    public HotbarUI hotbarUI;

    public int selectedSlot = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        while (slots.Count < hotbarSize)
        {
            slots.Add(new InventorySlot(null, 0));
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public bool AddItem(ItemData item, int amount)
    {
        if (item == null) return false;

        int currentTotal = 0;
        int itemSlotIndex = -1;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                currentTotal += slots[i].amount;
                itemSlotIndex = i;
            }
        }

        if (currentTotal >= item.maxStack)
            return false;

        int amountToAdd = Mathf.Min(amount, item.maxStack - currentTotal);

        if (itemSlotIndex != -1)
        {
            slots[itemSlotIndex].amount += amountToAdd;
        }
        else
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty())
                {
                    slots[i].item = item;
                    slots[i].amount = amountToAdd;
                    break;
                }
            }
        }

        RefreshUI();
        return true;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= hotbarSize) return;

        selectedSlot = index;
        RefreshUI();
    }

    public void SelectNext()
    {
        selectedSlot++;

        if (selectedSlot >= hotbarSize)
            selectedSlot = 0;

        RefreshUI();
    }

    public void SelectPrevious()
    {
        selectedSlot--;

        if (selectedSlot < 0)
            selectedSlot = hotbarSize - 1;

        RefreshUI();
    }

    public InventorySlot GetSelectedSlot()
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return null;

        return slots[selectedSlot];
    }

    public void UseSelectedItem()
    {
        InventorySlot slot = GetSelectedSlot();

        if (slot == null || slot.IsEmpty())
        {
            Debug.Log("No item selected.");
            return;
        }

        ItemData item = slot.item;

        Debug.Log(item.useDebugMessage);

        if (item.affectsStats)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();

                if (playerStats != null)
                {
                    switch (item.effectType)
                    {
                        case ItemEffectType.Add:
                            playerStats.AddStat(item.statType, item.statAmount);
                            Debug.Log(item.itemName + " used. Added " + item.statAmount + " to " + item.statType);
                            break;

                        case ItemEffectType.Subtract:
                            playerStats.SubtractStat(item.statType, item.statAmount);
                            Debug.Log(item.itemName + " used. Subtracted " + item.statAmount + " from " + item.statType);
                            break;

                        case ItemEffectType.None:
                        default:
                            Debug.LogWarning(item.itemName + " has no valid effect type.");
                            return;
                    }

                    Debug.Log("New " + item.statType + " value: " + playerStats.GetStat(item.statType));
                }
                else
                {
                    Debug.LogWarning("PlayerStats component not found on Player.");
                    return;
                }
            }
            else
            {
                Debug.LogWarning("Player with tag 'Player' not found.");
                return;
            }
        }

        if (item.consumable)
        {
            slot.amount--;

            if (slot.amount <= 0)
            {
                slot.Clear();
            }
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (hotbarUI != null)
        {
            hotbarUI.Refresh(slots, selectedSlot);
        }
    }
}