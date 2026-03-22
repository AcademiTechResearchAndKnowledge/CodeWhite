using UnityEngine;

public class PointManager : MonoBehaviour
{
    public static PointManager Instance;

    [Header("Progress")]
    public int requiredPoints = 5;
    private int currentPoints = 0;

    [Header("Point Spawning")]
    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private float spawnHeight = 1f;

    [Header("Spawn Validation")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private int spawnAttempts = 20;

    [Header("Portal")]
    [SerializeField] private RandomPortalSpawner portalSpawner;

    private bool portalSpawned = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnPoint(); // spawn first point when scene starts
    }

    public void AddPoint()
    {
        if (portalSpawned) return;

        currentPoints++;
        Debug.Log($"Points: {currentPoints}/{requiredPoints}");

        if (currentPoints >= requiredPoints)
        {
            SpawnPortal();
        }
        else
        {
            SpawnPoint(); // spawn next point
        }
    }

    private void SpawnPoint()
    {
        if (pointPrefab == null || spawnArea == null)
        {
            Debug.LogWarning("PointManager: Missing pointPrefab or spawnArea.");
            return;
        }

        for (int i = 0; i < spawnAttempts; i++)
        {
            Vector3 randomPoint = GetRandomPointInBox(spawnArea.bounds);

            Vector3 rayOrigin = randomPoint + Vector3.up * raycastHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, groundMask))
            {
                Vector3 spawnPos = hit.point + Vector3.up * 0.1f;

                Instantiate(pointPrefab, spawnPos, Quaternion.identity);
                Debug.Log("Spawned point on valid surface!");
                return;
            }
        }

        Debug.LogWarning("Failed to find valid spawn point. Increase attempts or fix groundMask.");
    }

    private Vector3 GetRandomPointInBox(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.center.y,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void SpawnPortal()
    {
        if (portalSpawner != null)
        {
            portalSpawner.SpawnPortalRandom();
            portalSpawned = true;
            Debug.Log("Portal spawned!");
        }
        else
        {
            Debug.LogWarning("PointManager: No portalSpawner assigned.");
        }
    }
}