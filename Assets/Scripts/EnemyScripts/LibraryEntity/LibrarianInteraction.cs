using UnityEngine;

// Notice we inherit from Interactable instead of MonoBehaviour
public class LibrarianInteraction : Interactable
{
    [Tooltip("Drag the script that handles the Librarian's anxiety/entity spawning here.")]
    public LibrarianManager librarianManager;

    // We override the Interact method that your PlayerInteraction script calls when 'F' is pressed
    public override void Interact()
    {
        TryGiveBook();
    }

    private void TryGiveBook()
    {
        ObjectiveInventorySlot selectedSlot = ObjectiveInventoryManager.Instance.GetSelectedSlot();

        if (selectedSlot == null || selectedSlot.IsEmpty())
        {
            Debug.Log("Librarian: 'You aren't holding anything! Go find my books!'");
            return;
        }

        ObjectiveItemData selectedItem = selectedSlot.item;

        if (selectedItem.bookType == LibraryBookType.None)
        {
            Debug.Log($"Librarian: 'I don't want your {selectedItem.itemName}. I only want my books!'");
            return;
        }

        // Submit the book to process anxiety and entities
        librarianManager.SubmitBook(selectedItem.bookType);

        // Remove exactly 1 of this item from the inventory
        ObjectiveInventoryManager.Instance.RemoveItem(selectedItem, 1);
    }
}