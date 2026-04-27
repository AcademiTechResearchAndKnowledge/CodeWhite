using UnityEngine;

public class RandomPortalSpawner : MonoBehaviour
{
    [Header("Portal")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private bool spawnOnlyOnce = true;

    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName;

    [Header("Where it can spawn")]
    [SerializeField] private BoxCollider[] spawnAreas;
    [SerializeField] private LayerMask portalspawnMask;
    [SerializeField] private LayerMask ceilingMask;

    [Header("Spawn search")]
    [SerializeField] private int attempts = 25;
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private float groundOffset = 0.05f;

    private bool spawned;

    public void SpawnPortalRandom()
    {
        if (portalPrefab == null || spawned && spawnOnlyOnce) return;
        if (spawnAreas == null || spawnAreas.Length == 0) return;

        for (int i = 0; i < attempts; i++)
        {
            BoxCollider area = spawnAreas[Random.Range(0, spawnAreas.Length)];
            Vector3 randomPoint = RandomPointInBox(area.bounds);

            Vector3 rayOrigin = randomPoint + Vector3.up * raycastHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, portalspawnMask))
            {
                Vector3 spawnPos;

               
                Vector3 ceilingOrigin = hit.point + Vector3.down * 0.5f;

                if (Physics.Raycast(ceilingOrigin, Vector3.up, out RaycastHit ceilingHit, raycastHeight * 2f, ceilingMask))
                {

                    spawnPos = ceilingHit.point;
                }
                else
                {
    
                    spawnPos = hit.point + Vector3.up * groundOffset;
                }

                GameObject portalInstance = Instantiate(portalPrefab, spawnPos, Quaternion.identity);

                PortalNextStage portalScript = portalInstance.GetComponent<PortalNextStage>();
                if (portalScript != null)
                {
                    portalScript.nextSceneName = nextSceneName;
                }

                spawned = true;
                return;
            }
        }

        Debug.LogWarning("[RandomPortalSpawner] Failed to find valid spawn point.");
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