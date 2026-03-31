using UnityEngine;
using System.Collections;

public class PuzzleDrawer : Interactable
{
    public enum DrawerType { TopDrawer, BottomDrawer }

    [Header("Drawer Info")]
    public DrawerType drawerType;

    [Header("Animation")]
    public Animator drawerAnimator;
    public float animationStepDelay = 0.5f;

    [Header("Spawn Point")]
    public Transform itemSpawnPoint;
    public float lootSpawnDelay = 0.2f;

    private bool isOpen = false;
    private bool isBusy = false;
    private bool hasBeenSearched = false;

    void Start()
    {
        // Register this specific drawer layer to the manager pool
        if (LighterPuzzleManager.instance != null)
        {
            LighterPuzzleManager.instance.RegisterDrawer(this);
        }
    }

    public override void Interact()
    {
        if (isBusy) return;
        base.Interact();

        if (drawerAnimator == null) return;

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
        }
        else
        {
            drawerAnimator.SetInteger("C", 5);
        }

        // Only check for the lighter the VERY FIRST time this drawer is opened
        if (!hasBeenSearched)
        {
            yield return new WaitForSeconds(lootSpawnDelay);

            // Ask the manager if we should spawn the lighter here
            if (LighterPuzzleManager.instance.TrySpawnLighter(this))
            {
                SpawnLighter();
            }
            hasBeenSearched = true;
        }

        yield return new WaitForSeconds(animationStepDelay - lootSpawnDelay);

        if (drawerType == DrawerType.TopDrawer) drawerAnimator.SetInteger("C", 2);
        else drawerAnimator.SetInteger("C", 6);

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

    void SpawnLighter()
    {
        if (itemSpawnPoint == null || LighterPuzzleManager.instance.lighterPrefab == null) return;

        GameObject lighter = Instantiate(LighterPuzzleManager.instance.lighterPrefab, itemSpawnPoint.position, itemSpawnPoint.rotation);
        lighter.transform.SetParent(itemSpawnPoint, true);
        lighter.tag = "Interactable";

        Rigidbody rb = lighter.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider itemCollider = lighter.GetComponent<Collider>();
        Collider drawerCollider = GetComponent<Collider>();

        if (itemCollider != null && drawerCollider != null)
        {
            Physics.IgnoreCollision(itemCollider, drawerCollider, true);
        }

        Debug.Log(gameObject.name + " spawned the lighter!");
    }
    public void ResetSearchState()
    {
        // This allows the drawer to be searched again!
        hasBeenSearched = false;
    }
}