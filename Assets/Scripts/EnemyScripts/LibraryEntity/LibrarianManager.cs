using UnityEngine;

public class LibrarianManager : MonoBehaviour
{
    [Header("Anxiety Settings")]
    [SerializeField] private float currentAnxiety = 0f;
    private const float maxAnxiety = 100f;
    private const float anxietyThreshold = 70f;

    [Header("Entity Spawning")]
    public GameObject huntingEntityPrefab;
    public Transform entitySpawnPoint;

    public void SubmitBook(LibraryBookType submittedBookType)
    {
        switch (submittedBookType)
        {
            case LibraryBookType.Signed:
                ModifyAnxiety(-5f);
                Debug.Log("Book Accepted: Librarian is satisfied. Anxiety reduced by 5%.");
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
        if (huntingEntityPrefab != null && entitySpawnPoint != null)
        {
            Instantiate(huntingEntityPrefab, entitySpawnPoint.position, entitySpawnPoint.rotation);
            Debug.Log("[Entity Action] The Hunting Entity has been spawned into the world!");
        }
    }
}