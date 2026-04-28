using System.Collections.Generic;
using UnityEngine;

public class ObjectiveInventoryManager : MonoBehaviour
{
    public static ObjectiveInventoryManager Instance;

    public int objectiveSize = 2;
    public List<ObjectiveInventorySlot> slots = new List<ObjectiveInventorySlot>();
    public ObjectiveUI objectiveUI;

    public AudioSource audioSource;

    public int selectedSlot = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
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

    // -------------------------------------------------------------
    // NOTE: The Update() method listening for 'E' was removed here! 
    // Your PlayerInventoryController handles all keyboard input safely.
    // -------------------------------------------------------------

    public bool AddItem(ObjectiveItemData item, int amount)
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
        {
            Debug.Log("Objective stack is full!");
            return false;
        }

        int amountToAdd = Mathf.Min(amount, item.maxStack - currentTotal);

        if (itemSlotIndex != -1)
        {
            slots[itemSlotIndex].amount += amountToAdd;
        }
        else
        {
            bool foundEmptySlot = false;

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty())
                {
                    slots[i].item = item;
                    slots[i].amount = amountToAdd;
                    foundEmptySlot = true;
                    break;
                }
            }

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

        HandController hand = FindFirstObjectByType<HandController>();
        if (hand != null)
        {
            ObjectiveInventorySlot slot = GetSelectedSlot();
            if (slot != null && !slot.IsEmpty())
            {
                hand.EquipItemByData(slot.item);
            }
            else
            {
                hand.UnequipAll();
            }
        }
    }

    public void DeselectAll()
    {
        selectedSlot = -1;
        RefreshUI();

        HandController hand = FindFirstObjectByType<HandController>();
        if (hand != null)
        {
            hand.UnequipAll();
        }
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

    // --- THE MASTER DROP METHOD ---
    public void DropSelectedItem(Vector3 dropPosition)
    {
        // ---------------------------------------------------------
        // NEW: PREVENT DROPPING WHILE READING!
        // ---------------------------------------------------------
        BookInspectionUI inspectUI = FindFirstObjectByType<BookInspectionUI>();
        if (inspectUI != null && inspectUI.IsOpen())
        {
            Debug.Log("[Drop] Cannot drop the book while inspecting it!");
            return; // Stop the drop!
        }

        ObjectiveInventorySlot slot = GetSelectedSlot();

        if (slot == null || slot.IsEmpty()) return;

        ObjectiveItemData itemToDrop = slot.item;

        // --- RESTRICT DROPPING TO BOOKS ONLY ---
        if (itemToDrop.bookType == LibraryBookType.None)
        {
            Debug.Log($"[Drop] You cannot drop {itemToDrop.itemName}.");
            return; // Stops the code right here!
        }

        // Safety check: Does this item actually have a 3D model assigned to drop?
        if (itemToDrop.worldPrefab == null)
        {
            Debug.LogWarning($"[Drop] Cannot drop {itemToDrop.itemName}. No worldPrefab assigned in the Inspector!");
            return;
        }

        // 1. Spawn the physical item in the world
        GameObject droppedObject = Instantiate(itemToDrop.worldPrefab, dropPosition, Quaternion.identity);

        // 2. Give the dropped object our EXACT cloned data so it remembers if it was forged!
        ObjectiveItemPickup pickupScript = droppedObject.GetComponent<ObjectiveItemPickup>();
        if (pickupScript != null)
        {
            pickupScript.itemData = itemToDrop;
        }

        // 3. Tell the physical book to show the correct color
        LibraryBook visualScript = droppedObject.GetComponent<LibraryBook>();
        if (visualScript != null)
        {
            visualScript.selectedVisualIndex = itemToDrop.visualIndex;

            // Force the book to refresh its color immediately so it doesn't change on the floor
            visualScript.UpdateVisuals();
        }

        // 4. Remove the item from your inventory
        slot.amount--;

        if (slot.amount <= 0)
        {
            slot.Clear();
            DeselectAll(); // Removes the model from the player's hand
        }

        RefreshUI();
        Debug.Log($"[Drop] Successfully dropped: {itemToDrop.itemName}");
    }

    // --- THE MASTER USE METHOD ---
    public void UseSelectedItem()
    {
        ObjectiveInventorySlot slot = GetSelectedSlot();

        if (slot == null || slot.IsEmpty()) return;

        ObjectiveItemData itemToUse = slot.item;
        Debug.Log("Using item: " + itemToUse.itemName);

        if (audioSource != null && itemToUse.useSound != null)
        {
            audioSource.PlayOneShot(itemToUse.useSound);
        }

        // 1. IS IT A BOOK?
        // If it's a book, open the UI and STOP reading the rest of this code.
        if (itemToUse.bookType != LibraryBookType.None)
        {
            BookInspectionUI inspectUI = FindFirstObjectByType<BookInspectionUI>();
            if (inspectUI != null)
            {
                inspectUI.OpenInspection(itemToUse);
                return; // <-- This protects the book from being "consumed"
            }
        }

        // 2. DOES IT SPAWN A PORTAL?
        if (itemToUse.spawnsPortal)
        {
            RandomPortalSpawner spawner = FindFirstObjectByType<RandomPortalSpawner>();
            if (spawner != null)
            {
                spawner.SpawnPortalRandom();
                Debug.Log("Portal spawned from objective inventory!");
            }
        }

        // 3. IS IT CONSUMABLE?
        if (itemToUse.consumable)
        {
            slot.amount--;

            if (slot.amount <= 0)
            {
                slot.Clear();
                DeselectAll(); // Unequips the visual from the player's hand
            }

            RefreshUI();
        }

        // 4. TRIGGER TUTORIAL
        TutorialManager tutorial = FindFirstObjectByType<TutorialManager>();
        if (tutorial != null)
        {
            tutorial.ItemUsed();
        }
    }

    public bool RemoveItem(ObjectiveItemData item, int amountToRemove)
    {
        int remainingToRemove = amountToRemove;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == item)
            {
                if (slots[i].amount >= remainingToRemove)
                {
                    slots[i].amount -= remainingToRemove;
                    if (slots[i].amount <= 0) slots[i].Clear();

                    RefreshUI();
                    return true;
                }
                else
                {
                    remainingToRemove -= slots[i].amount;
                    slots[i].Clear();
                }
            }
        }

        RefreshUI();
        return remainingToRemove <= 0;
    }
}