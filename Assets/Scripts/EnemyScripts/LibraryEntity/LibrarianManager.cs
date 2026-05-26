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
    [Tooltip("No need to assign this in the inspector anymore! The script will find it dynamically.")]
    public TextMeshProUGUI hintText;
    public float hintDisplayTime = 3f;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // NEW FIX: Look for a GameObject named exactly "PlayerHintText" anywhere in the scene
        GameObject foundTextObject = GameObject.Find("PlayerHintText");

        if (foundTextObject != null)
        {
            hintText = foundTextObject.GetComponent<TextMeshProUGUI>();
            Debug.Log("LibrarianManager: Successfully found and connected to the Player's Hint Text!");
        }
        else
        {
            // BACKUP PLAN: If the name doesn't match, try to find the Player by Tag and search their children
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                hintText = player.GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log("LibrarianManager: Found Player tag, grabbing first TextMeshPro component found in children.");
            }
        }

        // Clean up the text at the start of the level
        if (hintText != null)
        {
            hintText.text = "";
        }
        else
        {
            Debug.LogError("LibrarianManager: CRITICAL! Could not find the Player's Hint Text UI in this scene. Check your GameObject names!");
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
                if (currentAnxiety >= anxietyThreshold)
                {
                    Debug.Log("[Librarian Action] The Librarian begins to taunt!");
                }
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