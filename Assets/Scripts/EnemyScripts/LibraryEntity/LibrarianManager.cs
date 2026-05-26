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
                }

                if (signedBooksSubmitted >= requiredSignedBooks)
                {
                    SpawnPortal();
                }
                break;

            case LibraryBookType.Unsigned:
                ModifyAnxiety(5f);
                break;

            case LibraryBookType.Forged:
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
        {
            hintText.text = "";
        }
    }
}