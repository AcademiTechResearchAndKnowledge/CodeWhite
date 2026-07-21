using UnityEngine;
using System;
using System.Collections;

public class RainbowDoorInteractable : Interactable
{
    [Header("Puzzle Requirements")]
    [SerializeField] private ObjectiveItemData requiredKeyData;

    [Header("Entity Settings")]
    [SerializeField] private GameObject entity_1;

    [Header("Entity Spawn Effects")]
    [Tooltip("The dedicated AudioSource to play the spawn sound (e.g., placed at the spawn point for 3D spatial audio).")]
    [SerializeField] private AudioSource spawnAudioSource;
    [Tooltip("The sound to play when the entity is spawned.")]
    [SerializeField] private AudioClip spawnSound;
    [Tooltip("Volume for the spawn sound.")]
    [Range(0f, 1f)][SerializeField] private float spawnVolume = 1.0f;

    [Header("Entity Despawn Effects")]
    [Tooltip("The dedicated AudioSource to play the despawn sound.")]
    [SerializeField] private AudioSource despawnAudioSource;
    [Tooltip("The particle effect to spawn when the entity disappears.")]
    [SerializeField] private GameObject despawnParticlePrefab;
    [Tooltip("The sound to play when the entity despawns (e.g., chase end music).")]
    [SerializeField] private AudioClip despawnSound;
    [Tooltip("Volume for the despawn sound.")]
    [Range(0f, 1f)][SerializeField] private float despawnVolume = 1.0f;

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

    private CutsceneManager cutsceneManager;

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
        CacheCutsceneManager();
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

            // Failsafe: If the cutscene manager wasn't found during Start() because of teleporting, find it now.
            if (cutsceneManager == null)
            {
                CacheCutsceneManager();
            }

            if (cutsceneManager != null)
            {
                cutsceneManager.ActivateCutscene();
            }
            else
            {
                Debug.LogWarning("CutsceneManager was not found in the scene to play the door cutscene!");
            }

            if (doorGen == null)
                CacheDoorGen();

            if (doorGen != null)
                doorGen.SpawnKeyOnce();
        }

        if (isOpened) return;

        ObjectiveInventorySlot selectedSlot = ObjectiveInventoryManager.Instance.GetSelectedSlot();
        bool hasCorrectKeySelected = selectedSlot != null && !selectedSlot.IsEmpty() && selectedSlot.item == requiredKeyData;

        if (hasCorrectKeySelected)
        {
            isOpened = true;

            ObjectiveInventoryManager.Instance.RemoveItem(requiredKeyData, 1);
            ObjectiveInventoryManager.Instance.DeselectAll();

            if (spawnedEntity != null)
            {
                if (despawnParticlePrefab != null)
                {
                    Instantiate(despawnParticlePrefab, spawnedEntity.transform.position, spawnedEntity.transform.rotation);
                }

                if (despawnSound != null)
                {
                    if (despawnAudioSource != null)
                    {
                        despawnAudioSource.PlayOneShot(despawnSound, despawnVolume);
                    }
                    else
                    {
                        Play2DAudioFallback(despawnSound, despawnVolume);
                    }
                }

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

    void CacheCutsceneManager()
    {
        cutsceneManager = FindFirstObjectByType<CutsceneManager>();
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

        if (spawnSound != null)
        {
            if (spawnAudioSource != null)
            {
                spawnAudioSource.PlayOneShot(spawnSound, spawnVolume);
            }
            else
            {
                Play2DAudioFallback(spawnSound, spawnVolume);
            }
        }
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

    private void Play2DAudioFallback(AudioClip clip, float volume)
    {
        GameObject tempAudioObject = new GameObject("TempRuntimeAudio");
        AudioSource source = tempAudioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();
        Destroy(tempAudioObject, clip.length);
    }

    private void OnEnable()
    {
        DeathMenu.OnPlayerRestart += ResetInteractionState;
    }

    private void OnDisable()
    {
        DeathMenu.OnPlayerRestart -= ResetInteractionState;
    }

    private void ResetInteractionState()
    {
        hasInteracted = false;

        isOpened = false;
        entitySpawned = false;

        if (spawnedEntity != null)
        {
            Destroy(spawnedEntity);
            spawnedEntity = null;
        }
    }
}