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

        if (drawerAnimator == null)
        {
            Debug.LogWarning("No Animator assigned on " + gameObject.name);
            return;
        }

        if (!isOpen)
        {
            StartCoroutine(OpenDrawerRoutine());
        }
        else
        {
            StartCoroutine(CloseDrawerRoutine());
        }
    }

    IEnumerator OpenDrawerRoutine()
    {
        isBusy = true;

        if (drawerType == DrawerType.TopDrawer)
        {
            drawerAnimator.SetInteger("C", 1);

            if (!spawnOnlyOnce || !hasSpawned)
            {
                yield return new WaitForSeconds(lootSpawnDelay);
                SpawnRandomItem();
                hasSpawned = true;
            }

            yield return new WaitForSeconds(animationStepDelay - lootSpawnDelay);
            drawerAnimator.SetInteger("C", 2);
        }
        else
        {
            drawerAnimator.SetInteger("C", 5);

            if (!spawnOnlyOnce || !hasSpawned)
            {
                yield return new WaitForSeconds(lootSpawnDelay);
                SpawnRandomItem();
                hasSpawned = true;
            }

            yield return new WaitForSeconds(animationStepDelay - lootSpawnDelay);
            drawerAnimator.SetInteger("C", 6);
        }

        isOpen = true;
        isBusy = false;
    }

    IEnumerator CloseDrawerRoutine()
    {
        isBusy = true;

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

        isOpen = false;
        isBusy = false;
    }

    void SpawnRandomItem()
    {
        if (itemSpawnPoint == null)
        {
            Debug.LogWarning("No itemSpawnPoint assigned on " + gameObject.name);
            return;
        }

        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogWarning("No possibleItems assigned on " + gameObject.name);
            return;
        }

        if (Random.value < chanceToSpawnNothing)
        {
            Debug.Log(gameObject.name + " spawned nothing.");
            return;
        }

        int randomIndex = Random.Range(0, possibleItems.Length);
        GameObject chosenItem = possibleItems[randomIndex];

        currentSpawnedItem = Instantiate(chosenItem, itemSpawnPoint.position, itemSpawnPoint.rotation);

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

        Debug.Log(gameObject.name + " spawned " + chosenItem.name);
    }
}