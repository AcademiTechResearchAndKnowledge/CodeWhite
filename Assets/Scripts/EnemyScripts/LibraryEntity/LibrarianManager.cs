using UnityEngine;

public class LibrarianManager : MonoBehaviour
{
    [Header("Anxiety Settings")]
    [SerializeField] private float currentAnxiety = 0f;
    private const float maxAnxiety = 100f;
    private const float anxietyThreshold = 70f;

    [Header("Entity Spawning")]
    public GameObject huntingEntityPrefab;

    [Tooltip("Drag all your empty spawn point GameObjects into this list.")]
    public Transform[] entitySpawnPoints;

    [Header("Level Progress")]
    [SerializeField] private int signedBooksSubmitted = 0;
    private const int requiredSignedBooks = 10;

    public void SubmitBook(LibraryBookType submittedBookType)
    {
        switch (submittedBookType)
        {
            case LibraryBookType.Signed:
                ModifyAnxiety(-5f);
                signedBooksSubmitted++;

                Debug.Log($"Book Accepted: Librarian is satisfied. Anxiety reduced by 5%. ({signedBooksSubmitted}/{requiredSignedBooks} Signed Books)");

                if (signedBooksSubmitted >= requiredSignedBooks)
                {
                    SpawnPortal();
                }
                break;

            case LibraryBookType.Unsigned:
                ModifyAnxiety(5f);
                Debug.Log("Book Rejected: Librarian is mad! No signature found. Anxiety increased by 5%.");

                if (currentAnxiety >= anxietyThreshold)
                {
                    Debug.Log("[Librarian Action] The Librarian begins to taunt, scare, and pressure the player!");
                }
                break;

            case LibraryBookType.Forged:
                Debug.Log("Book Rejected: FORGED SIGNATURE DETECTED. Librarian is enraged!");
                SpawnHuntingEntity();
                break;
        }
    }

    private void ModifyAnxiety(float amount)
    {
        currentAnxiety = Mathf.Clamp(currentAnxiety + amount, 0, maxAnxiety);
        Debug.Log($"[Anxiety System] Current Anxiety is now: {currentAnxiety}%");
    }

    private void SpawnHuntingEntity()
    {
        // Check if we have the prefab AND at least one spawn point in the array
        if (huntingEntityPrefab != null && entitySpawnPoints != null && entitySpawnPoints.Length > 0)
        {
            // 1. Pick a random number between 0 and the total number of spawn points
            int randomIndex = Random.Range(0, entitySpawnPoints.Length);

            // 2. Select the spawn point at that random index
            Transform selectedSpawnPoint = entitySpawnPoints[randomIndex];

            // 3. Spawn the Corrector at the chosen location
            Instantiate(huntingEntityPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
            Debug.Log($"[Entity Action] The Corrector has spawned at point: {selectedSpawnPoint.name}!");
        }
        else
        {
            Debug.LogWarning("Cannot spawn entity! Please assign the Entity Prefab and at least one Spawn Point in the inspector.");
        }
    }

    private void SpawnPortal()
    {
        Debug.Log("[Level Complete] 10 Signed Books submitted! The Portal is now spawning...");
    }
}