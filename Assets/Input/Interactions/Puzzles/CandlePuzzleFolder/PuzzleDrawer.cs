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

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip drawerOpenSound;
    [Tooltip("Delay in seconds before the open sound plays.")]
    public float openSoundDelay = 0f;

    public AudioClip drawerCloseSound;
    [Tooltip("Delay in seconds before the close sound plays.")]
    public float closeSoundDelay = 0f;

    private bool isOpen = false;
    private bool hasBeenSearched = false;

    private Coroutine activeCoroutine;
    private float lastInteractTime = 0f;
    private float interactCooldown = 0.25f;

    void Start()
    {
        if (LighterPuzzleManager.instance != null)
        {
            LighterPuzzleManager.instance.RegisterDrawer(this);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public override void Interact()
    {
        if (Time.time - lastInteractTime < interactCooldown)
            return;

        lastInteractTime = Time.time;
        base.Interact();

        if (drawerAnimator == null) return;

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

        PlaySound(drawerOpenSound, openSoundDelay);

        if (drawerType == DrawerType.TopDrawer)
        {
            drawerAnimator.SetInteger("C", 1);
        }
        else
        {
            drawerAnimator.SetInteger("C", 5);
        }

        yield return new WaitForSeconds(lootSpawnDelay);

        if (!hasBeenSearched)
        {
            if (LighterPuzzleManager.instance.TrySpawnLighter(this))
            {
                SpawnLighter();
            }
            hasBeenSearched = true;
        }

        float waitTime = Mathf.Max(0, animationStepDelay - lootSpawnDelay);
        yield return new WaitForSeconds(waitTime);

        if (drawerType == DrawerType.TopDrawer)
            drawerAnimator.SetInteger("C", 2);
        else
            drawerAnimator.SetInteger("C", 6);

        activeCoroutine = null;
    }

    IEnumerator CloseDrawerRoutine()
    {
        isOpen = false;

        PlaySound(drawerCloseSound, closeSoundDelay);

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
        hasBeenSearched = false;
    }

    private void PlaySound(AudioClip clip, float delay = 0f)
    {
        if (audioSource != null && clip != null)
        {
            if (delay > 0f)
            {
                StartCoroutine(PlaySoundCO(clip, delay));
            }
            else
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    private IEnumerator PlaySoundCO(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}