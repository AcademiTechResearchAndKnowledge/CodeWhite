using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RandomPortalSpawner : MonoBehaviour
{
    [Header("Portal")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private bool spawnOnlyOnce = true;

    public enum PortalOrientation
    {
        Vertical,
        Horizontal
    }

    [Header("Portal Orientation")]
    private PortalOrientation portalOrientation = PortalOrientation.Vertical;

    [Header("Portal Sequence Settings")]
    [SerializeField] private float duration = 6f;
    [SerializeField] private float liftHeight = 2f;
    [SerializeField] private float lookHeight = 10f;
    [SerializeField] private float fadeStart = 0.6f;
    [SerializeField] private float fadeSpeed = 1f;

    [Header("Level Progress (PERSISTENT)")]
    [SerializeField] private int levelCounter = 0;

    [Header("Scene Exclusions")]
    [SerializeField] private string[] excludedScenes;

    [Header("Manual Boss Scenes (IN ORDER)")]
    [SerializeField] private string[] bossScenes;

    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider[] spawnAreas;
    [SerializeField] private LayerMask portalspawnMask;
    [SerializeField] private LayerMask ceilingMask;
    [SerializeField] private LayerMask wallMask;

    [Header("Spawn Search")]
    [SerializeField] private int attempts = 25;
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private float wallRaycastDistance = 50f;
    [SerializeField] private float groundOffset = 0.05f;
    [SerializeField] private float wallOffset = 0.05f;

    private bool spawned;
    private static RandomPortalSpawner instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        spawned = false;
        spawnAreas = FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);
    }

    private Quaternion BuildRotation(Vector3 normal)
    {
        Vector3 up = normal;

        Vector3 forward = Vector3.Cross(Vector3.up, up);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.Cross(Vector3.forward, up);

        forward.Normalize();

        return Quaternion.LookRotation(forward, up);
    }

    public void SpawnPortalAt(Vector3 position, PortalOrientation orientation)
    {
        if (portalPrefab == null) return;

        portalOrientation = orientation;
        levelCounter++;

        List<string> validScenes = GetValidScenes();
        if (validScenes.Count == 0) return;

        GameObject portalInstance = Instantiate(portalPrefab, position, Quaternion.Euler(90f, 0f, 0f));

        ApplyPortalSetup(portalInstance, validScenes);
        spawned = true;
    }

    public void SpawnPortalRandom(PortalOrientation orientation)
    {
        portalOrientation = orientation;

        if (portalPrefab == null) return;
        if (spawned && spawnOnlyOnce) return;
        if (spawnAreas == null || spawnAreas.Length == 0) return;

        levelCounter++;

        List<string> validScenes = GetValidScenes();
        if (validScenes.Count == 0) return;

        for (int i = 0; i < attempts; i++)
        {
            BoxCollider area = spawnAreas[Random.Range(0, spawnAreas.Length)];
            Vector3 randomPoint = RandomPointInBox(area.bounds);
            Vector3 rayOrigin = randomPoint + Vector3.up * raycastHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, portalspawnMask))
            {
                Vector3 spawnPos;
                Quaternion rot;

                if (portalOrientation == PortalOrientation.Horizontal)
                {
                    Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
                    Vector3 wallOrigin = hit.point + Vector3.up * 1f;

                    spawnPos = hit.point + hit.normal * groundOffset;
                    rot = BuildRotation(hit.normal);

                    foreach (Vector3 dir in dirs)
                    {
                        if (Physics.Raycast(wallOrigin, dir, out RaycastHit wallHit, wallRaycastDistance, wallMask))
                        {
                            spawnPos = wallHit.point + wallHit.normal * wallOffset;
                            rot = BuildRotation(wallHit.normal);
                            break;
                        }
                    }
                }
                else
                {
                    Vector3 ceilingOrigin = hit.point + Vector3.down * 0.5f;

                    if (Physics.Raycast(ceilingOrigin, Vector3.up, out RaycastHit ceilingHit, raycastHeight * 2f, ceilingMask))
                    {
                        spawnPos = ceilingHit.point + ceilingHit.normal * groundOffset;
                        rot = BuildRotation(ceilingHit.normal);
                    }
                    else
                    {
                        spawnPos = hit.point + hit.normal * groundOffset;
                        rot = BuildRotation(hit.normal);
                    }
                }

                GameObject portalInstance = Instantiate(portalPrefab, spawnPos, rot);

                ApplyPortalSetup(portalInstance, validScenes);

                spawned = true;
                return;
            }
        }
    }

    private void ApplyPortalSetup(GameObject portalInstance, List<string> validScenes)
    {
        PortalNextStage portal = portalInstance.GetComponentInChildren<PortalNextStage>();

        if (portal != null)
        {
            portal.SetOrientation(portalOrientation == PortalOrientation.Horizontal
                ? PortalNextStage.PortalOrientation.Horizontal
                : PortalNextStage.PortalOrientation.Vertical);

            portal.SetSequenceSettings(duration, liftHeight, lookHeight, fadeStart, fadeSpeed);
            portal.SetLevel(levelCounter);
            portal.SetExcludedScenes(excludedScenes);

            bool isBoss = (levelCounter % 10 == 0);

            if (isBoss)
                portal.SetForcedScene(GetBossScene(levelCounter));
            else
                portal.SetForcedScene(validScenes[Random.Range(0, validScenes.Count)]);
        }
    }

    private string GetBossScene(int level)
    {
        if (bossScenes == null || bossScenes.Length == 0)
            return "";

        int index = Mathf.Clamp((level / 10) - 1, 0, bossScenes.Length - 1);
        return bossScenes[index];
    }

    private List<string> GetValidScenes()
    {
        List<string> scenes = new List<string>();
        int count = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (IsExcluded(name)) continue;

            scenes.Add(name);
        }

        return scenes;
    }

    private bool IsExcluded(string sceneName)
    {
        if (excludedScenes == null) return false;

        foreach (var s in excludedScenes)
        {
            if (s == sceneName)
                return true;
        }

        return false;
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