using UnityEngine;
using System;
using System.Collections;

public class RainbowDoorInteractable : Interactable
{
    [SerializeField] private GameObject entity_1;

    public Animator doorAnimator;
    public float openTime = 1f;

    private Transform entity_1_spawn;
    private bool spawnFound;

    private bool isOpened;
    private bool entitySpawned;

    private GameObject spawnedEntity;

    private Transform player;
    private bool playerFound;

    private RandomPortalSpawner portalSpawner;
    private doorsGen doorGen;

    [HideInInspector] public Action onPuzzleComplete;

    private bool hasInteracted;

    void Awake()
    {
        CacheSpawnPoint();
        CachePortalSpawner();
        CacheDoorGen();
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
        if (!hasInteracted)
        {
            hasInteracted = true;

            if (doorGen == null)
                CacheDoorGen();

            if (doorGen != null)
                doorGen.SpawnKeyOnce();
        }

        if (isOpened) return;

        if (DoorPuzzleHandler.instance != null && DoorPuzzleHandler.instance.hasKey)
        {
            isOpened = true;

            if (spawnedEntity != null)
            {
                Destroy(spawnedEntity);
                spawnedEntity = null;
            }

            StartCoroutine(OpenDoorRoutine());
        }
        else
        {
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
        }

        onPuzzleComplete?.Invoke();

        if (portalSpawner != null)
        {
            portalSpawner.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
        }

        GameObject toDestroy = transform.parent != null ? transform.parent.gameObject : gameObject;
        Destroy(toDestroy);
    }

    void CacheDoorGen()
    {
        doorGen = FindFirstObjectByType<doorsGen>();
    }

    void CachePortalSpawner()
    {
        portalSpawner = FindFirstObjectByType<RandomPortalSpawner>();
    }

    void TriggerEntity()
    {
        if (entitySpawned) return;
        if (entity_1 == null) return;
        if (!spawnFound || entity_1_spawn == null) return;

        spawnedEntity = Instantiate(entity_1, entity_1_spawn.position, entity_1_spawn.rotation);
        entitySpawned = true;
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
            }
        }
        catch (UnityException)
        {
            spawnFound = false;
        }
    }
}