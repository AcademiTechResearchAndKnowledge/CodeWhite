using UnityEngine;

public class BookPuzzle : Interactable
{
    [HideInInspector] public bool isCorrectBook;

    public override void Interact()
    {
        if (isCorrectBook)
        {
            Debug.Log($"Interacted with CORRECT book: {name}");
            BookPuzzleManager.Instance.OnCorrectBook(this);
        }
        else
        {
            Debug.Log($"Interacted with WRONG book: {name} | Correct book: {BookPuzzleManager.Instance.correctBook.name}");
            BookPuzzleManager.Instance.OnWrongBook(this);
        }
    }
}