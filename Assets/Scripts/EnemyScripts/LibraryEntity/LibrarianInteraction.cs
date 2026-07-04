using UnityEngine;

// Notice we inherit from Interactable instead of MonoBehaviour
public class LibrarianInteraction : Interactable
{
    [Tooltip("Drag the script that handles the Librarian's anxiety/entity spawning here.")]
    public LibrarianManager librarianManager;

    public override void Interact()
    {
        TryGiveBook();
    }

    private void TryGiveBook()
    {
        ObjectiveInventorySlot selectedSlot = ObjectiveInventoryManager.Instance.GetSelectedSlot();

        // 1. Check if the player is holding absolutely nothing
        if (selectedSlot == null || selectedSlot.IsEmpty())
        {
            Debug.Log("Librarian: 'You aren't holding anything! Go find my books!'");

            if (librarianManager != null)
            {
                // Displays the default message: "You don't have a book to submit!"
                librarianManager.PlayNoBookError();
            }

            return;
        }

        ObjectiveItemData selectedItem = selectedSlot.item;

        // 2. Check if the player is holding an item, but it isn't a book
        if (selectedItem.bookType == LibraryBookType.None)
        {
            Debug.Log($"Librarian: 'I don't want your {selectedItem.itemName}. I only want my books!'");

            if (librarianManager != null)
            {
                // Displays a dynamic custom message tailored to what they are holding
                librarianManager.PlayNoBookError($"I don't want a {selectedItem.itemName}. Find my books!");
            }

            return;
        }

        // 3. Submit the book to process anxiety and entities
        if (librarianManager != null)
        {
            librarianManager.SubmitBook(selectedItem.bookType);
        }

        // Remove exactly 1 of this item from the inventory
        ObjectiveInventoryManager.Instance.RemoveItem(selectedItem, 1);
    }
}