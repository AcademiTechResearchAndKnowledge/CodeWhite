using UnityEngine;
using System;
using System.Collections;

public class RainbowDoorInteractable : Interactable
{
    [Header("Puzzle Requirements")]
    [SerializeField] private ObjectiveItemData requiredKeyData;

    [Header("Entity Settings")]
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

        // 1. Grab the currently selected slot from the Inventory Manager
        ObjectiveInventorySlot selectedSlot = ObjectiveInventoryManager.Instance.GetSelectedSlot();

        // 2. Check if the slot contains an item and if that item matches our required key
        bool hasCorrectKeySelected = selectedSlot != null && !selectedSlot.IsEmpty() && selectedSlot.item == requiredKeyData;

        if (hasCorrectKeySelected)
        {
            isOpened = true;

            // 3. Remove the key from the inventory and clear the player's active selection/hand visual
            ObjectiveInventoryManager.Instance.RemoveItem(requiredKeyData, 1);
            ObjectiveInventoryManager.Instance.DeselectAll();

            if (spawnedEntity != null)
            {
                Destroy(spawnedEntity);
                spawnedEntity = null;
            }

            StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            // Triggers the monster if the user doesn't have the key selected
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