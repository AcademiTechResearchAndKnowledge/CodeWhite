using UnityEngine;
using TMPro;
using System.Collections;

public class LibrarianManager : MonoBehaviour
{
    public static LibrarianManager instance;
    public RandomPortalSpawner RPS;

    [Header("Debug")]
    [SerializeField] private bool puzzleFinishDebugTrigger;

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
    public TextMeshProUGUI hintText;
    public float hintDisplayTime = 3f;

    private bool puzzleCompleted;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (RPS == null)
            RPS = FindFirstObjectByType<RandomPortalSpawner>();

        if (hintText != null)
            hintText.text = "";
    }

    private void Update()
    {
        if (puzzleFinishDebugTrigger && !puzzleCompleted)
        {
            puzzleFinishDebugTrigger = false;
            CompletePuzzle();
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
                        audioSource.PlayOneShot(correctBookSFX);

                    int booksRemaining = requiredSignedBooks - signedBooksSubmitted;
                    ShowDialogue($"Thank you! I need {booksRemaining} more books!");
                }
                else
                {
                    CompletePuzzle();
                }
                break;

            case LibraryBookType.Unsigned:
                ModifyAnxiety(5f);
                ShowDialogue("An unsigned book? This is not welcome here.");
                break;

            case LibraryBookType.Forged:
                ShowDialogue("You gave me a forged book! You will pay!");
                SpawnHuntingEntity();
                break;
        }
    }

    private void CompletePuzzle()
    {
        if (puzzleCompleted) return;
        puzzleCompleted = true;

        ShowDialogue("You found them all! Opening the portal...");

        if (audioSource != null && puzzleCompleteSFX != null)
            audioSource.PlayOneShot(puzzleCompleteSFX);

        SpawnPortal();
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
        if (RPS != null)
        {
            RPS.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
        }
        else
        {
            Debug.LogWarning("LibrarianManager: RandomPortalSpawner not found in scene.");
        }
    }

    public void PlayNoBookError(string message = "You don't have a book to submit!")
    {
        if (audioSource != null && noBookSFX != null)
            audioSource.PlayOneShot(noBookSFX);

        ShowDialogue(message);
    }

    private void ShowDialogue(string message)
    {
        if (hintText != null)
        {
            hintText.text = message;
            StopAllCoroutines();
            StartCoroutine(ClearHintText());
        }
    }

    private IEnumerator ClearHintText()
    {
        yield return new WaitForSeconds(hintDisplayTime);

        if (hintText != null)
            hintText.text = "";
    }
}