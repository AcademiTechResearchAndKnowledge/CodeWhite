using UnityEngine;
using System.Collections;

public class ChestLoot : Interactable
{
    [Header("Animation")]
    public Animator chestAnimator;
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

        // Call the base Interact method from your Interactable class
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

        // Transition from ChestIdle to ChestOpen
        chestAnimator.SetInteger("C", 1);

        if (!spawnOnlyOnce || !hasSpawned)
        {
            // Wait for the specific moment in the animation to spawn the loot
            yield return new WaitForSeconds(lootSpawnDelay);
            SpawnRandomItem();
            hasSpawned = true;
        }

        // Wait for the rest of the open animation to finish
        yield return new WaitForSeconds(animationStepDelay - lootSpawnDelay);

        // Transition to ChestOpen_Idle
        chestAnimator.SetInteger("C", 2);

        isOpen = true;
        isBusy = false;
    }

    IEnumerator CloseChestRoutine()
    {
        isBusy = true;

        // Transition from ChestOpen_Idle to ChestClose
        chestAnimator.SetInteger("C", 3);

        yield return new WaitForSeconds(animationStepDelay);

        // Transition from ChestClose back to ChestIdle
        chestAnimator.SetInteger("C", 4);



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

        // Parent the item to the spawn point so it moves with the chest lid/base if necessary
        currentSpawnedItem.transform.SetParent(itemSpawnPoint, true);

        Rigidbody rb = currentSpawnedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Freeze the physics so the item doesn't fly out or clip through the chest
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider itemCollider = currentSpawnedItem.GetComponent<Collider>();
        Collider chestCollider = GetComponent<Collider>();

        if (itemCollider != null && chestCollider != null)
        {
            // Prevent the spawned item from colliding with the chest itself
            Physics.IgnoreCollision(itemCollider, chestCollider, true);
        }

        Debug.Log(gameObject.name + " spawned " + chosenItem.name);
    }
}