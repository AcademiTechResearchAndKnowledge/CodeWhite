using UnityEngine;
using System.Collections;

public class DrawerLoot : Interactable
{
    public enum DrawerType
    {
        TopDrawer,
        BottomDrawer
    }

    [Header("Drawer Info")]
    public DrawerType drawerType;

    [Header("Animation")]
    public Animator drawerAnimator;
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
    private bool hasSpawned = false;
    private GameObject currentSpawnedItem;
    private Coroutine activeCoroutine;

    private float lastInteractTime = 0f;
    private float interactCooldown = 0.25f;

    public override void Interact()
    {
        if (Time.time - lastInteractTime < interactCooldown)
            return;

        lastInteractTime = Time.time;
        base.Interact();

        if (drawerAnimator == null)
        {
            Debug.LogWarning("No Animator assigned on " + gameObject.name);
            return;
        }

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        if (!isOpen)
        {
            activeCoroutine = StartCoroutine(OpenDrawerRoutine());
        }
        else
        {
            activeCoroutine = StartCoroutine(CloseDrawerRoutine());
        }
    }

    IEnumerator OpenDrawerRoutine()
    {
        isOpen = true;

        if (drawerType == DrawerType.TopDrawer)
        {
            drawerAnimator.SetInteger("C", 1);

            if (!spawnOnlyOnce || !hasSpawned)
            {
                yield return new WaitForSeconds(lootSpawnDelay);
                SpawnContent();
                hasSpawned = true;
            }

            float waitTime = Mathf.Max(0, animationStepDelay - lootSpawnDelay);
            yield return new WaitForSeconds(waitTime);

            drawerAnimator.SetInteger("C", 2);
        }
        else
        {
            drawerAnimator.SetInteger("C", 5);

            if (!spawnOnlyOnce || !hasSpawned)
            {
                yield return new WaitForSeconds(lootSpawnDelay);
                SpawnContent();
                hasSpawned = true;
            }

            float waitTime = Mathf.Max(0, animationStepDelay - lootSpawnDelay);
            yield return new WaitForSeconds(waitTime);

            drawerAnimator.SetInteger("C", 6);
        }

        activeCoroutine = null;
    }

    IEnumerator CloseDrawerRoutine()
    {
        isOpen = false;

        if (drawerType == DrawerType.TopDrawer)
        {
            drawerAnimator.SetInteger("C", 3);
            yield return new WaitForSeconds(animationStepDelay);
            drawerAnimator.SetInteger("C", 4);
        }
        else
        {
            drawerAnimator.SetInteger("C", 7);
            yield return new WaitForSeconds(animationStepDelay);
            drawerAnimator.SetInteger("C", 8);
        }

        activeCoroutine = null;
    }

    void SpawnContent()
    {
        if (itemSpawnPoint == null) return;

        if (entityPrefab != null && Random.value < chanceToSpawnEntity)
        {
            InstantiateAndSetupObject(entityPrefab);
            return;
        }

        if (Random.value < chanceToSpawnNothing) return;

        if (possibleItems == null || possibleItems.Length == 0) return;

        int randomIndex = Random.Range(0, possibleItems.Length);
        InstantiateAndSetupObject(possibleItems[randomIndex]);
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
        Collider drawerCollider = GetComponent<Collider>();

        if (itemCollider != null && drawerCollider != null)
        {
            Physics.IgnoreCollision(itemCollider, drawerCollider, true);
        }
    }
}