using UnityEngine;

public class RainbowDoorInteractable : Interactable
{
    [SerializeField] private GameObject entity_1;

    private Transform entity_1_spawn;
    private bool spawnFound = false;

    private bool isOpened = false;
    private bool entitySpawned = false;

    private GameObject spawnedEntity; 

    private Transform player;
    private bool playerFound = false;

    void Awake()
    {
        CacheSpawnPoint();
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
            Debug.Log("Rainbow door opened!");
            isOpened = true;

   
            if (spawnedEntity != null)
            {
                Destroy(spawnedEntity);
                spawnedEntity = null;
                Debug.Log("Entity destroyed because door was opened.");
            }
        }
        else
        {
            Debug.Log("The rainbow door is locked. You need a key.");
            TriggerEntity();
        }
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