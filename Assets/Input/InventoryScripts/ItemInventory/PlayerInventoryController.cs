using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventoryController : MonoBehaviour
{
    public Transform dropPoint;
    public HandController handController;

    private int globalSlotIndex = 0;
    private const int MAX_TOTAL_SLOTS = 7;

    private string currentDisplayedItem = "";

    private void Start()
    {
        UpdateInventorySelection();
    }

    private void Update()
    {
        if (InventoryManager.Instance == null || ObjectiveInventoryManager.Instance == null) return;

        HandleScrollSelection();
        HandleNumberSelection();
        HandleUseInput();
        HandleDropInput();

        CheckHandVisualUpdates();
    }

    void CheckHandVisualUpdates()
    {
        string intendedItemName = "";

        if (globalSlotIndex < 5)
        {
            var slot = InventoryManager.Instance.GetSelectedSlot();
            if (slot != null && !slot.IsEmpty())
            {
                intendedItemName = slot.item.itemName;
            }
        }
        else
        {
            var slot = ObjectiveInventoryManager.Instance.GetSelectedSlot();
            if (slot != null && !slot.IsEmpty())
            {
                intendedItemName = slot.item.itemName;
            }
        }

        if (currentDisplayedItem != intendedItemName)
        {
            currentDisplayedItem = intendedItemName;

            if (handController != null)
            {
                if (string.IsNullOrEmpty(intendedItemName))
                    handController.UnequipAll();
                else
                    handController.EquipItemByName(intendedItemName);
            }
        }
    }

    void HandleScrollSelection()
    {
        if (Mouse.current == null) return;

        float scrollValue = Mouse.current.scroll.ReadValue().y;

        if (scrollValue > 0f)
        {
            globalSlotIndex--;
            if (globalSlotIndex < 0) globalSlotIndex = MAX_TOTAL_SLOTS - 1;
            UpdateInventorySelection();
        }
        else if (scrollValue < 0f)
        {
            globalSlotIndex++;
            if (globalSlotIndex >= MAX_TOTAL_SLOTS) globalSlotIndex = 0;
            UpdateInventorySelection();
        }
    }

    void HandleNumberSelection()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) SetGlobalSlot(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) SetGlobalSlot(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) SetGlobalSlot(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) SetGlobalSlot(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) SetGlobalSlot(4);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) SetGlobalSlot(5);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) SetGlobalSlot(6);
    }

    void SetGlobalSlot(int index)
    {
        globalSlotIndex = index;
        UpdateInventorySelection();
    }

    void UpdateInventorySelection()
    {
        if (globalSlotIndex < 5)
        {
            InventoryManager.Instance.SelectSlot(globalSlotIndex);
            ObjectiveInventoryManager.Instance.DeselectAll();
        }
        else
        {
            InventoryManager.Instance.DeselectAll();
            ObjectiveInventoryManager.Instance.SelectSlot(globalSlotIndex - 5);
        }
    }

    void HandleUseInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (globalSlotIndex < 5)
            {
                InventoryManager.Instance.UseSelectedItem();
            }
            else
            {
                Debug.Log("Objective items are used automatically at specific locations!");
            }
        }
    }

    void HandleDropInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;

            if (globalSlotIndex < 5)
            {
                InventoryManager.Instance.DropSelectedItem(spawnPosition);
            }
            else
            {
                ObjectiveInventoryManager.Instance.DropSelectedItem(spawnPosition);
            }
        }
    }
}