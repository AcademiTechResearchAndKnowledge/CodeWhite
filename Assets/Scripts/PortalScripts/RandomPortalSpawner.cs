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
    [SerializeField] private PortalOrientation portalOrientation = PortalOrientation.Vertical;

    [Header("Level Progress (PERSISTENT)")]
    [SerializeField] private int levelCounter = 0;

    [Header("Scene Exclusions")]
    [SerializeField] private string[] excludedScenes;

    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider[] spawnAreas;
    [SerializeField] private LayerMask portalspawnMask;
    [SerializeField] private LayerMask ceilingMask;

    [Header("Spawn Search")]
    [SerializeField] private int attempts = 25;
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private float groundOffset = 0.05f;

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

    public void SpawnPortalRandom()
    {
        if (portalPrefab == null || (spawned && spawnOnlyOnce)) return;
        if (spawnAreas == null || spawnAreas.Length == 0) return;

        levelCounter++;

        Debug.Log("SPAWNER LEVEL = " + levelCounter);

        List<string> validScenes = GetValidScenes();

        if (validScenes.Count == 0)
        {
            Debug.LogError("No valid scenes available after exclusions!");
            return;
        }

        for (int i = 0; i < attempts; i++)
        {
            BoxCollider area = spawnAreas[Random.Range(0, spawnAreas.Length)];
            Vector3 randomPoint = RandomPointInBox(area.bounds);

            Vector3 rayOrigin = randomPoint + Vector3.up * raycastHeight;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f, portalspawnMask))
            {
                Vector3 spawnPos;
                Quaternion rotation;

                if (portalOrientation == PortalOrientation.Horizontal)
                {
                    spawnPos = hit.point + Vector3.up * groundOffset;
                    rotation = Quaternion.Euler(90f, 0f, 0f);
                }
                else
                {
                    Vector3 ceilingOrigin = hit.point + Vector3.down * 0.5f;

                    if (Physics.Raycast(ceilingOrigin, Vector3.up, out RaycastHit ceilingHit, raycastHeight * 2f, ceilingMask))
                        spawnPos = ceilingHit.point;
                    else
                        spawnPos = hit.point + Vector3.up * groundOffset;

                    rotation = Quaternion.identity;
                }

                GameObject portalInstance = Instantiate(portalPrefab, spawnPos, rotation);

                PortalNextStage portal = portalInstance.GetComponentInChildren<PortalNextStage>();

                if (portal != null)
                {
                    portal.SetLevel(levelCounter);
                }

                spawned = true;
                return;
            }
        }

        Debug.LogWarning("Failed to find valid spawn point");
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