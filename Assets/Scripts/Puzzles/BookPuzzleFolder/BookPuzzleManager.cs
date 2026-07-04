using System.Collections.Generic;
using UnityEngine;

public class BookPuzzleManager : MonoBehaviour
{
    public static BookPuzzleManager Instance;

    [Header("Parent of all books")]
    public Transform booksParent; // assign the parent in inspector

    [HideInInspector] public BookPuzzle correctBook;
    private List<BookPuzzle> books = new List<BookPuzzle>();

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Automatically add BookPuzzle to every child
        if (booksParent != null)
        {
            foreach (Transform child in booksParent)
            {
                var bp = child.GetComponent<BookPuzzle>();
                if (bp == null)
                    bp = child.gameObject.AddComponent<BookPuzzle>();

                // Make sure the child also has Interactable
                if (child.GetComponent<Interactable>() == null)
                    child.gameObject.AddComponent<Interactable>();

                books.Add(bp);
            }
        }

        if (books.Count == 0)
        {
            Debug.LogWarning("No books found under the parent!");
            return;
        }

        SetupPuzzle();
    }

    public void SetupPuzzle()
    {
        // Pick a random book as correct
        int randomIndex = Random.Range(0, books.Count);
        correctBook = books[randomIndex];

        // Assign correct/wrong flags and log all books
        foreach (var book in books)
        {
            //for debug purposes skiibidii
            book.isCorrectBook = (book == correctBook);
            //Debug.Log($"Book: {book.name} | IsCorrect: {book.isCorrectBook}");
        }

        Debug.Log($"Correct book is: {correctBook.name}");
    }

    public void OnCorrectBook(BookPuzzle book)
    {
        Debug.Log($"Interacted with CORRECT book: {book.name} | Puzzle Solved!");
        // Add puzzle logic keneme
    }

    public void OnWrongBook(BookPuzzle book)
    {
        Debug.Log($"Interacted with WRONG book: {book.name} | Correct book is: {correctBook.name}");
        // Optional feedback for wrong books
    }
}