using UnityEngine;
using System.Collections.Generic;

public class doorsGen : MonoBehaviour
{
    public GameObject blackDoorPrefab;
    public GameObject rainbowDoorPrefab;
    public GameObject whiteDoorPrefab;
    public GameObject keyPrefab;

    public Transform doorsParent;
    public Transform keysParent;
    public Transform floor;

    public int totalDoors = 20;
    public float spawnAreaSize = 50f;
    public float minDistanceBetweenDoors = 5f;

    public Material portalMaterialBase;
    public Material idlePortalMaterial;
    public Transform playerTransform;

    private List<Vector3> usedPositions = new List<Vector3>();
    private float floorTop;

    private GameObject rainbowDoor;

    private bool keySpawned;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }

        Collider floorCollider = floor.GetComponent<Collider>();
        floorTop = floorCollider.bounds.max.y;

        SpawnDoors();
    }

    void SpawnDoors()
    {
        List<GameObject> blackDoors = new List<GameObject>();

        int rainbowIndex = Random.Range(0, totalDoors);
        int whiteIndex = Random.Range(0, totalDoors);

        while (whiteIndex == rainbowIndex)
            whiteIndex = Random.Range(0, totalDoors);

        for (int i = 0; i < totalDoors; i++)
        {
            Vector3 pos = GetValidRandomPosition();
            if (pos == Vector3.zero) continue;

            pos.y = floorTop;

            GameObject prefabToSpawn = blackDoorPrefab;

            if (i == rainbowIndex) prefabToSpawn = rainbowDoorPrefab;
            else if (i == whiteIndex) prefabToSpawn = whiteDoorPrefab;

            GameObject door = Instantiate(prefabToSpawn, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0), doorsParent);

            AdjustToFloor(door);

            usedPositions.Add(door.transform.position);

            if (prefabToSpawn == rainbowDoorPrefab)
            {
                rainbowDoor = door;

                RainbowDoorInteractable rd = door.GetComponent<RainbowDoorInteractable>();
                if (rd != null)
                {
                    rd.onPuzzleComplete = OnRainbowDoorOpened;

                    if (rd.doorAnimator == null)
                        rd.doorAnimator = door.GetComponentInChildren<Animator>();
                }
            }
            else if (prefabToSpawn == blackDoorPrefab)
            {
                blackDoors.Add(door);
            }
        }

        for (int i = 0; i < blackDoors.Count; i += 2)
        {
            if (i + 1 >= blackDoors.Count) break;
            SetupPortalPair(blackDoors[i], blackDoors[i + 1]);
        }
    }

    public void SpawnKeyOnce()
    {
        if (keySpawned) return;
        keySpawned = true;
        SpawnKey();
    }

    void SpawnKey()
    {
        Vector3 pos = GetValidRandomPosition();

        if (pos == Vector3.zero)
            pos = GetFallbackPosition();

        pos.y = floorTop;

        GameObject key = Instantiate(keyPrefab, pos, Quaternion.identity, keysParent);
        AdjustToFloor(key);
    }

    Vector3 GetFallbackPosition()
    {
        Vector3 center = floor != null ? floor.position : Vector3.zero;

        Vector3 offset = new Vector3(
            Random.Range(-spawnAreaSize, spawnAreaSize),
            0,
            Random.Range(-spawnAreaSize, spawnAreaSize)
        );

        return center + offset;
    }

    private void OnRainbowDoorOpened()
    {
        if (rainbowDoor == null) return;

        RandomPortalSpawner spawner = FindFirstObjectByType<RandomPortalSpawner>();
        if (spawner == null) return;

        Vector3 spawnPos = GetDoorCenter(rainbowDoor);
        spawner.SpawnPortalAt(spawnPos, RandomPortalSpawner.PortalOrientation.Vertical);
    }

    Vector3 GetDoorCenter(GameObject door)
    {
        Renderer[] renderers = door.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            foreach (Renderer r in renderers)
                b.Encapsulate(r.bounds);
            return b.center;
        }

        return door.transform.position;
    }

    void SetupPortalPair(GameObject doorA, GameObject doorB)
    {
        Camera camA = new GameObject("PortalCam_A").AddComponent<Camera>();
        Camera camB = new GameObject("PortalCam_B").AddComponent<Camera>();

        camA.enabled = false;
        camB.enabled = false;

        RenderTexture texA = new RenderTexture(1024, 1024, 24);
        RenderTexture texB = new RenderTexture(1024, 1024, 24);

        camA.targetTexture = texA;
        camB.targetTexture = texB;

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            camA.fieldOfView = mainCam.fieldOfView;
            camB.fieldOfView = mainCam.fieldOfView;
            camA.nearClipPlane = mainCam.nearClipPlane;
            camB.nearClipPlane = mainCam.nearClipPlane;
            camA.farClipPlane = mainCam.farClipPlane;
            camB.farClipPlane = mainCam.farClipPlane;
        }

        PortalDoor pA = doorA.GetComponent<PortalDoor>() ?? doorA.AddComponent<PortalDoor>();
        PortalDoor pB = doorB.GetComponent<PortalDoor>() ?? doorB.AddComponent<PortalDoor>();

        pA.portalCamera = camA;
        pB.portalCamera = camB;

        pA.linkedDoor = pB;
        pB.linkedDoor = pA;

        pA.player = playerTransform;
        pB.player = playerTransform;

        pA.portalMaterialBase = portalMaterialBase;
        pB.portalMaterialBase = portalMaterialBase;

        pA.idlePortalMaterial = idlePortalMaterial;
        pB.idlePortalMaterial = idlePortalMaterial;
    }

    void AdjustToFloor(GameObject obj)
    {
        Collider col = obj.GetComponentInChildren<Collider>();
        if (col == null) return;

        float offset = floorTop - col.bounds.min.y;
        obj.transform.position += new Vector3(0f, offset, 0f);
    }

    Vector3 GetValidRandomPosition()
    {
        int attempts = 0;

        while (attempts < 200)
        {
            attempts++;

            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaSize, spawnAreaSize),
                0,
                Random.Range(-spawnAreaSize, spawnAreaSize)
            );

            if (IsValid(pos))
                return pos;
        }

        return Vector3.zero;
    }

    bool IsValid(Vector3 pos)
    {
        Vector2 p = new Vector2(pos.x, pos.z);

        foreach (Vector3 used in usedPositions)
        {
            Vector2 u = new Vector2(used.x, used.z);
            if (Vector2.Distance(p, u) < minDistanceBetweenDoors)
                return false;
        }

        return true;
    }
}