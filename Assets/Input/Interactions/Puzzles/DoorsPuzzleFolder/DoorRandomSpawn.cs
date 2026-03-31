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
        SpawnKey();
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

            GameObject prefabToSpawn = blackDoorPrefab;
            if (i == rainbowIndex) prefabToSpawn = rainbowDoorPrefab;
            else if (i == whiteIndex) prefabToSpawn = whiteDoorPrefab;

            GameObject door = Instantiate(
                prefabToSpawn,
                new Vector3(pos.x, 0f, pos.z),
                Quaternion.Euler(0, Random.Range(0f, 360f), 0),
                doorsParent
            );

            AdjustToFloor(door);
            usedPositions.Add(pos);

            if (prefabToSpawn == blackDoorPrefab)
                blackDoors.Add(door);
        }

        // Pair black doors
        for (int i = 0; i < blackDoors.Count; i += 2)
        {
            if (i + 1 >= blackDoors.Count) break;
            SetupPortalPair(blackDoors[i], blackDoors[i + 1]);
        }
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
        if (col != null)
        {
            float halfHeight = col.bounds.extents.y;
            obj.transform.position = new Vector3(
                obj.transform.position.x,
                floorTop + halfHeight,
                obj.transform.position.z
            );
        }
    }

    void SpawnKey()
    {
        Vector3 pos = GetValidRandomPosition();
        if (pos == Vector3.zero) return;

        GameObject key = Instantiate(keyPrefab, pos, Quaternion.identity, keysParent);
        AdjustToFloor(key);
    }

    Vector3 GetValidRandomPosition()
    {
        int attempts = 0;

        while (attempts < 100)
        {
            attempts++;

            Vector3 pos = new Vector3(
                Random.Range(-spawnAreaSize, spawnAreaSize),
                0,
                Random.Range(-spawnAreaSize, spawnAreaSize)
            );

            bool valid = true;

            foreach (Vector3 used in usedPositions)
            {
                if (Vector3.Distance(pos, used) < minDistanceBetweenDoors)
                {
                    valid = false;
                    break;
                }
            }

            if (valid) return pos;
        }

        return Vector3.zero;
    }
}