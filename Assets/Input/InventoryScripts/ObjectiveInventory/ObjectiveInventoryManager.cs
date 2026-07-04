using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        SceneManager.sceneLoaded += OnSceneLoaded;

        while (slots.Count < objectiveSize)
        {
            slots.Add(new ObjectiveInventorySlot(null, 0));
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearInventory();
        Debug.Log("[ObjectiveInventoryManager] New scene loaded. Inventory wiped.");
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

    public void DropSelectedItem(Vector3 dropPosition)
    {
        BookInspectionUI inspectUI = FindFirstObjectByType<BookInspectionUI>();
        if (inspectUI != null && inspectUI.IsOpen())
        {
            Debug.Log("[Drop] Cannot drop the book while inspecting it!");
            return;
        }

        ObjectiveInventorySlot slot = GetSelectedSlot();

        if (slot == null || slot.IsEmpty()) return;

        ObjectiveItemData itemToDrop = slot.item;

        if (itemToDrop.bookType == LibraryBookType.None)
        {
            Debug.Log($"[Drop] You cannot drop {itemToDrop.itemName}.");
            return;
        }

        if (itemToDrop.worldPrefab == null)
        {
            Debug.LogWarning($"[Drop] Cannot drop {itemToDrop.itemName}. No worldPrefab assigned in the Inspector!");
            return;
        }

        GameObject droppedObject = Instantiate(itemToDrop.worldPrefab, dropPosition, Quaternion.identity);

        ObjectiveItemPickup pickupScript = droppedObject.GetComponent<ObjectiveItemPickup>();
        if (pickupScript != null)
        {
            pickupScript.itemData = itemToDrop;
        }

        LibraryBook visualScript = droppedObject.GetComponent<LibraryBook>();
        if (visualScript != null)
        {
            visualScript.selectedVisualIndex = itemToDrop.visualIndex;
            visualScript.UpdateVisuals();
        }

        slot.amount--;

        if (slot.amount <= 0)
        {
            slot.Clear();
            DeselectAll();
        }

        RefreshUI();
        Debug.Log($"[Drop] Successfully dropped: {itemToDrop.itemName}");
    }

    public void UseSelectedItem()
    {
        BookInspectionUI inspectUI = FindFirstObjectByType<BookInspectionUI>();
        if (inspectUI != null && inspectUI.IsOpen())
        {
            Debug.Log("[Use] The book is already open! Closing it now.");
            inspectUI.CloseInspection();
            return;
        }

        ObjectiveInventorySlot slot = GetSelectedSlot();

        if (slot == null || slot.IsEmpty()) return;

        ObjectiveItemData itemToUse = slot.item;
        Debug.Log("Using item: " + itemToUse.itemName);

        if (audioSource != null && itemToUse.useSound != null)
        {
            audioSource.PlayOneShot(itemToUse.useSound);
        }

        if (itemToUse.bookType != LibraryBookType.None)
        {
            if (inspectUI != null)
            {
                inspectUI.OpenInspection(itemToUse);
                return;
            }
        }

        if (itemToUse.spawnsPortal)
        {
            RandomPortalSpawner spawner = FindFirstObjectByType<RandomPortalSpawner>();
            if (spawner != null)
            {
                spawner.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
                Debug.Log("Portal spawned from objective inventory!");
            }
        }

        if (itemToUse.consumable)
        {
            slot.amount--;

            if (slot.amount <= 0)
            {
                slot.Clear();
                DeselectAll();
            }

            RefreshUI();
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

    public void ClearInventory()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Clear();
        }

        DeselectAll();
        RefreshUI();
    }
}