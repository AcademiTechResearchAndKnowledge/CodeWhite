using UnityEngine;
using TMPro;
using System.Collections;

public class LibrarianManager : MonoBehaviour
{
    public static LibrarianManager instance;

    [Header("Anxiety Settings")]
    [SerializeField] private float currentAnxiety = 0f;
    private const float maxAnxiety = 100f;
    private const float anxietyThreshold = 70f;

    [Header("Entity Spawning")]
    public GameObject huntingEntityPrefab;
    public Transform[] entitySpawnPoints;

    [Header("Level Progress")]
    [SerializeField] private int signedBooksSubmitted = 0;
    private const int requiredSignedBooks = 10;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip correctBookSFX;
    public AudioClip puzzleCompleteSFX;
    public AudioClip noBookSFX;

    [Header("UI Settings")]
    [Tooltip("Drag the TextMeshPro UI object from this scene's Hierarchy into this slot.")]
    public TextMeshProUGUI hintText;
    public float hintDisplayTime = 3f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Clean up the text at the start of the level
        if (hintText != null)
        {
            hintText.text = "";
        }
        else
        {
            Debug.LogWarning("LibrarianManager: No Hint Text assigned in the Inspector!");
        }
    }

    public void SubmitBook(LibraryBookType submittedBookType)
    {
        switch (submittedBookType)
        {
            case LibraryBookType.Signed:
                ModifyAnxiety(-5f);
                signedBooksSubmitted++;

                if (signedBooksSubmitted < requiredSignedBooks)
                {
                    if (audioSource != null && correctBookSFX != null)
                    {
                        audioSource.PlayOneShot(correctBookSFX);
                    }

                    // 1. Tell the player how many books are remaining
                    int booksRemaining = requiredSignedBooks - signedBooksSubmitted;
                    ShowDialogue($"Thank you! I need {booksRemaining} more books!");
                }
                else
                {
                    ShowDialogue("You found them all! Opening the portal...");
                    SpawnPortal();
                }
                break;

            case LibraryBookType.Unsigned:
                ModifyAnxiety(5f);
                // 3. Tell the player you gave them an unsigned book
                ShowDialogue("An unsigned book? This is not welcome here.");
                break;

            case LibraryBookType.Forged:
                // 2. Tell the player they gave a forged book and will pay
                ShowDialogue("You gave me a forged book! You will pay!");
                SpawnHuntingEntity();
                break;
        }
    }

    private void ModifyAnxiety(float amount)
    {
        currentAnxiety = Mathf.Clamp(currentAnxiety + amount, 0, maxAnxiety);
    }

    private void SpawnHuntingEntity()
    {
        if (huntingEntityPrefab != null && entitySpawnPoints != null && entitySpawnPoints.Length > 0)
        {
            int randomIndex = Random.Range(0, entitySpawnPoints.Length);
            Transform selectedSpawnPoint = entitySpawnPoints[randomIndex];
            Instantiate(huntingEntityPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        }
    }

    private void SpawnPortal()
    {
        if (audioSource != null && puzzleCompleteSFX != null)
        {
            audioSource.PlayOneShot(puzzleCompleteSFX);
        }
    }

    public void PlayNoBookError(string message = "You don't have a book to submit!")
    {
        if (audioSource != null && noBookSFX != null)
        {
            audioSource.PlayOneShot(noBookSFX);
        }

        ShowDialogue(message);
    }

    // Helper method to display text and trigger the clear coroutine
    private void ShowDialogue(string message)
    {
        if (hintText != null)
        {
            hintText.text = message;
            StopAllCoroutines(); // Stops existing timers so messages don't disappear too quickly if spammed
            StartCoroutine(ClearHintText());
        }
    }

    private IEnumerator ClearHintText()
    {
        yield return new WaitForSeconds(hintDisplayTime);
        if (hintText != null)
        {
            hintText.text = "";
        }
    }
}