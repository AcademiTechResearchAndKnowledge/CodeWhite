using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventoryController : MonoBehaviour
{
    public Transform dropPoint;

    private void Update()
    {
        if (InventoryManager.Instance == null) return;

        HandleScrollSelection();
        HandleNumberSelection();
        HandleUseInput();
        HandleDropInput();
    }

    void HandleScrollSelection()
    {
        if (Mouse.current == null) return;

        float scrollValue = Mouse.current.scroll.ReadValue().y;

        if (scrollValue > 0f)
        {
            InventoryManager.Instance.SelectPrevious();
        }
        else if (scrollValue < 0f)
        {
            InventoryManager.Instance.SelectNext();
        }
    }

    void HandleNumberSelection()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            InventoryManager.Instance.SelectSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            InventoryManager.Instance.SelectSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            InventoryManager.Instance.SelectSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            InventoryManager.Instance.SelectSlot(3);

        if (Keyboard.current.digit5Key.wasPressedThisFrame)
            InventoryManager.Instance.SelectSlot(4);
    }

    void HandleUseInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            InventoryManager.Instance.UseSelectedItem();
        }
    }

    void HandleDropInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
            InventoryManager.Instance.DropSelectedItem(spawnPosition);
        }
    }
}