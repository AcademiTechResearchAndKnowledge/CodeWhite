using UnityEngine;
using System;
using System.Collections;

public class RainbowDoorInteractable : Interactable
{
    [SerializeField] private GameObject entity_1;

    [Header("Animator")]
    public Animator doorAnimator;
    public float openTime = 1f;

    private Transform entity_1_spawn;
    private bool spawnFound = false;

    private bool isOpened = false;
    private bool entitySpawned = false;

    private GameObject spawnedEntity;

    private Transform player;
    private bool playerFound = false;

    private RandomPortalSpawner portalSpawner;

    [HideInInspector] public Action onPuzzleComplete;

    void Awake()
    {
        CacheSpawnPoint();
        CachePortalSpawner();
    }

    void Start()
    {
        CachePlayer();
    }

    void Update()
    {
        if (isOpened) return;

        if (!playerFound)
            CachePlayer();
    }

    public override void Interact()
    {
        if (isOpened)
        {
            Debug.Log("Rainbow door is already open.");
            return;
        }

        if (DoorPuzzleHandler.instance != null && DoorPuzzleHandler.instance.hasKey)
        {
            Debug.Log("Rainbow door opened with key — destroying door.");
            isOpened = true;

            if (spawnedEntity != null)
            {
                Destroy(spawnedEntity);
                spawnedEntity = null;
                Debug.Log("Entity destroyed because door was opened.");
            }

            StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            Debug.Log("The rainbow door is locked. You need a key.");
            TriggerEntity();
        }
    }

    private IEnumerator OpenDoorRoutine()
    {
        if (doorAnimator != null)
        {
            doorAnimator.Play("Door - Open");
            yield return new WaitForSeconds(openTime);
        }
        else
        {
            yield return null;
            Debug.LogWarning("[RainbowDoorInteractable] No Animator assigned — destroying immediately.");
        }

        Debug.Log("[RainbowDoorInteractable] Invoking onPuzzleComplete. Is null: " + (onPuzzleComplete == null));
        onPuzzleComplete?.Invoke();

        if (portalSpawner != null)
        {
            Debug.Log("[RainbowDoorInteractable] Calling SpawnPortalRandom.");
            portalSpawner.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
        }
        else
        {
            Debug.LogWarning("[RainbowDoorInteractable] RandomPortalSpawner not found — skipping portal spawn.");
        }

        GameObject toDestroy = transform.parent != null ? transform.parent.gameObject : gameObject;
        Debug.Log($"[RainbowDoorInteractable] Destroying: {toDestroy.name}");
        Destroy(toDestroy);
    }

    private void CachePortalSpawner()
    {
        portalSpawner = FindFirstObjectByType<RandomPortalSpawner>();

        if (portalSpawner != null)
            Debug.Log("[RainbowDoorInteractable] RandomPortalSpawner found and cached.");
        else
            Debug.LogWarning("[RainbowDoorInteractable] RandomPortalSpawner not found in scene.");
    }

    void TriggerEntity()
    {
        if (entitySpawned)
        {
            Debug.Log("Entity already spawned.");
            return;
        }

        if (entity_1 == null)
        {
            Debug.LogWarning("Entity prefab is not assigned.");
            return;
        }

        if (!spawnFound || entity_1_spawn == null)
        {
            Debug.LogWarning("Entity spawn point not found or not assigned.");
            return;
        }

        spawnedEntity = Instantiate(entity_1, entity_1_spawn.position, entity_1_spawn.rotation);
        entitySpawned = true;

        Debug.Log("Entity spawned and is approaching the player...");
    }

    void CachePlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
        {
            player = p.transform;
            playerFound = true;
        }
        else
        {
            playerFound = false;
        }
    }

    void CacheSpawnPoint()
    {
        try
        {
            GameObject spawn = GameObject.FindGameObjectWithTag("EntitySpawn");

            if (spawn != null)
            {
                entity_1_spawn = spawn.transform;
                spawnFound = true;
            }
            else
            {
                spawnFound = false;
                Debug.LogWarning("No GameObject with tag 'EntitySpawn' found in scene.");
            }
        }
        catch (UnityException e)
        {
            spawnFound = false;
            Debug.LogError("Missing Tag in Unity Tag Manager: 'EntitySpawn'\n" + e.Message);
        }
    }
}