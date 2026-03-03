using UnityEngine;

public class RandomPortalSpawner : MonoBehaviour
{
    [Header("Portal")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private bool spawnOnlyOnce = true;

    [Header("Where it can spawn")]
    [SerializeField] private BoxCollider[] spawnAreas;   // set these as trigger boxes in the world
    [SerializeField] private LayerMask portalspawnMask;       // ground layer(s)

    [Header("Spawn search")]
    [SerializeField] private int attempts = 25;          // attempts until portal tries to spawn
    [SerializeField] private float raycastHeight = 50f;  // how high above the point we raycast from
    [SerializeField] private float groundOffset = 0.05f; // raise portal a tiny bit above ground

    private bool spawned;

    public void SpawnPortalRandom()
    {
        if (portalPrefab == null)
        {
            Debug.LogError("[RandomPortalSpawner] portalPrefab not assigned.");
            return;
        }

        if (spawnOnlyOnce && spawned) return;

        if (spawnAreas == null || spawnAreas.Length == 0)
        {
            Debug.LogError("[RandomPortalSpawner] Assign at least 1 BoxCollider in spawnAreas.");
            return;
        }

        for (int i = 0; i < attempts; i++)
        {
            // pick a random area
            BoxCollider area = spawnAreas[Random.Range(0, spawnAreas.Length)];
            Vector3 randomPoint = RandomPointInBox(area.bounds);

            // raycast down to find ground
            Vector3 rayOrigin = randomPoint + Vector3.up * raycastHeight;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, portalspawnMask))
            {
                Vector3 spawnPos = hit.point + Vector3.up * groundOffset;

                Instantiate(portalPrefab, spawnPos, Quaternion.identity);
                spawned = true;
                return;
            }
        }

        Debug.LogWarning("[RandomPortalSpawner] Failed to find valid ground point. Increase attempts or fix portalspawnMask/spawnAreas.");
    }

    private Vector3 RandomPointInBox(Bounds b)
    {
        return new Vector3(
            Random.Range(b.min.x, b.max.x),
            Random.Range(b.min.y, b.max.y),
            Random.Range(b.min.z, b.max.z)
        );
    }
}