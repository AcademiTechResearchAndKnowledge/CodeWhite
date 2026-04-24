using UnityEngine;
using System.Collections;

public class ChestLoot : Interactable
{
    [Header("Animation")]
    public Animator chestAnimator;
    public float animationStepDelay = 0.5f;

    [Header("Spawn Point")]
    public Transform itemSpawnPoint;

    [Header("Jumpscare Entity Settings")]
    [Tooltip("Drag the Entity Prefab here.")]
    public GameObject entityPrefab;
    [Range(0f, 1f)]
    [Tooltip("Independent chance to spawn the entity (0.1 = 10% chance)")]
    public float chanceToSpawnEntity = 0.1f;

    [Header("Possible Loot")]
    public GameObject[] possibleItems;

    [Header("Timing")]
    public float lootSpawnDelay = 0.2f;

    [Range(0f, 1f)]
    public float chanceToSpawnNothing = 0.3f;

    [Header("Loot Options")]
    public bool spawnOnlyOnce = true;

    private bool isOpen = false;
    private bool isBusy = false;
    private bool hasSpawned = false;
    private GameObject currentSpawnedItem;

    public override void Interact()
    {
        if (isBusy)
            return;

        base.Interact();

        if (chestAnimator == null)
        {
            Debug.LogWarning("No Animator assigned on " + gameObject.name);
            return;
        }

        if (!isOpen)
        {
            StartCoroutine(OpenChestRoutine());
        }
        else
        {
            StartCoroutine(CloseChestRoutine());
        }
    }

    IEnumerator OpenChestRoutine()
    {
        isBusy = true;

        chestAnimator.SetInteger("C", 1);

        if (!spawnOnlyOnce || !hasSpawned)
        {
            yield return new WaitForSeconds(lootSpawnDelay);
            SpawnContent();
            hasSpawned = true;
        }

        yield return new WaitForSeconds(animationStepDelay - lootSpawnDelay);

        chestAnimator.SetInteger("C", 2);

        isOpen = true;
        isBusy = false;
    }

    IEnumerator CloseChestRoutine()
    {
        isBusy = true;

        chestAnimator.SetInteger("C", 3);

        yield return new WaitForSeconds(animationStepDelay);

        chestAnimator.SetInteger("C", 4);

        isOpen = false;
        isBusy = false;
    }

    void SpawnContent()
    {
        if (itemSpawnPoint == null)
        {
            Debug.LogWarning("No itemSpawnPoint assigned on " + gameObject.name);
            return;
        }

        if (entityPrefab != null && Random.value < chanceToSpawnEntity)
        {
            InstantiateAndSetupObject(entityPrefab);
            Debug.Log(gameObject.name + " spawned the Entity!");
            return;
        }

        if (Random.value < chanceToSpawnNothing)
        {
            Debug.Log(gameObject.name + " spawned nothing.");
            return;
        }

        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogWarning("No possibleItems assigned on " + gameObject.name);
            return;
        }

        int randomIndex = Random.Range(0, possibleItems.Length);
        InstantiateAndSetupObject(possibleItems[randomIndex]);
        Debug.Log(gameObject.name + " spawned " + possibleItems[randomIndex].name);
    }

    void InstantiateAndSetupObject(GameObject prefabToSpawn)
    {
        currentSpawnedItem = Instantiate(prefabToSpawn, itemSpawnPoint.position, itemSpawnPoint.rotation);
        currentSpawnedItem.transform.SetParent(itemSpawnPoint, true);

        Rigidbody rb = currentSpawnedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider itemCollider = currentSpawnedItem.GetComponent<Collider>();
        Collider chestCollider = GetComponent<Collider>();

        if (itemCollider != null && chestCollider != null)
        {
            Physics.IgnoreCollision(itemCollider, chestCollider, true);
        }
    }
}