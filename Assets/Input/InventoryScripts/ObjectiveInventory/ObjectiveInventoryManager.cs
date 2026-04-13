using System.Collections.Generic;
using UnityEngine;

public class ObjectiveInventoryManager : MonoBehaviour
{
    public static ObjectiveInventoryManager Instance;

    public int objectiveSize = 2;
    public List<ObjectiveInventorySlot> slots = new List<ObjectiveInventorySlot>();
    public ObjectiveUI objectiveUI;

    public int selectedSlot = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        while (slots.Count < objectiveSize)
        {
            slots.Add(new ObjectiveInventorySlot(null, 0));
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public bool AddItem(ObjectiveItemData item, int amount)
    {
        if (item == null) return false;

        int currentTotal = 0;
        int itemSlotIndex = -1;

        // Check if item already exists in the inventory
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                currentTotal += slots[i].amount;
                itemSlotIndex = i;
            }
        }

        // Check if we've hit the stack limit
        if (currentTotal >= item.maxStack)
        {
            Debug.Log("Objective stack is full!");
            return false;
        }

        int amountToAdd = Mathf.Min(amount, item.maxStack - currentTotal);

        // Add to existing slot
        if (itemSlotIndex != -1)
        {
            slots[itemSlotIndex].amount += amountToAdd;
        }
        else // Or find a new empty slot
        {
            bool foundEmptySlot = false; // <-- WE ADDED THIS TRACKER

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty())
                {
                    slots[i].item = item;
                    slots[i].amount = amountToAdd;
                    foundEmptySlot = true; // <-- MARK AS FOUND
                    break;
                }
            }

            // THE FIX: If it looped through all slots and found nothing, fail the pickup.
            if (!foundEmptySlot)
            {
                Debug.Log("Inventory is completely full! No empty slots left.");
                return false;
            }
        }

        RefreshUI();
        return true;
    }

    public void SelectSlot(int index)
    {
        selectedSlot = index;
        RefreshUI();
    }

    public void DeselectAll()
    {
        selectedSlot = -1;
        RefreshUI();
    }

    public ObjectiveInventorySlot GetSelectedSlot()
    {
        if (selectedSlot < 0 || selectedSlot >= slots.Count)
            return null;

        return slots[selectedSlot];
    }

    public void RefreshUI()
    {
        if (objectiveUI != null)
        {
            objectiveUI.Refresh(slots, selectedSlot);
        }
    }

    public void DropSelectedItem(Vector3 dropPosition)
    {
        ObjectiveInventorySlot slot = GetSelectedSlot();

        if (slot == null || slot.IsEmpty()) return;

        Debug.Log("Cannot drop objective item: " + slot.item.itemName);
    }

    // Allows quest managers to delete specific items
    public bool RemoveItem(ObjectiveItemData item, int amountToRemove)
    {
        int remainingToRemove = amountToRemove;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                if (slots[i].amount >= remainingToRemove)
                {
                    // The slot has enough to cover the removal
                    slots[i].amount -= remainingToRemove;
                    if (slots[i].amount <= 0) slots[i].Clear();

                    RefreshUI();
                    return true;
                }
                else
                {
                    // The slot doesn't have enough, take what it has and keep looking
                    remainingToRemove -= slots[i].amount;
                    slots[i].Clear();
                }
            }
        }

        RefreshUI();
        // Returns true if we successfully removed all requested items
        return remainingToRemove <= 0;
    }
}