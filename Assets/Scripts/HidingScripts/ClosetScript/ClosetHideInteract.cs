using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClosetHideInteract : MonoBehaviour
{
    public bool CanInteract = true;

    [Header("White Lady Settings")]
    public float safeHideDistance = 12f;

    // --- Old AI References ---
    private EntityDetector entity;
    private EntityAi entityAi;
    private EntityWondering entityWondering;

    // --- New AI Reference ---
    private WhiteLady whiteLady;

    public float inputDelay = 2f;
    private bool inputLocked = false;

    private ClosetHidingSystem currentCloset;

    // --- INSTANT HIDE FIX ---
    private bool isTransitioningToHide = false;

    // The White Lady will now read this as TRUE the exact millisecond you press F
    public bool IsHiding => (currentCloset != null && currentCloset.InsideCloset) || isTransitioningToHide;

    void Start()
    {
        FindEntityReferences();
    }

    void FindEntityReferences()
    {
        entity = Object.FindFirstObjectByType<EntityDetector>();
        if (entity != null)
        {
            entityAi = entity.GetComponent<EntityAi>();
            entityWondering = entity.GetComponent<EntityWondering>();
        }

        whiteLady = Object.FindFirstObjectByType<WhiteLady>();
    }

    void Update()
    {
        if (inputLocked) return;

        if (entity == null && whiteLady == null)
        {
            FindEntityReferences();
        }

        // --- EXIT CLOSET ---
        if (Keyboard.current.gKey.wasPressedThisFrame && currentCloset != null && currentCloset.InsideCloset)
        {
            isTransitioningToHide = false; // Player is coming out!
            CanInteract = true;
            StartCoroutine(InputDelay());
            StartCoroutine(currentCloset.GoOutsideCloset_CO());
            return;
        }

        // --- ENTER CLOSET ---
        if (Keyboard.current.fKey.wasPressedThisFrame && CanInteract)
        {
            if (Camera.main == null) return;

            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 5))
            {
                ClosetHidingSystem closet = hit.collider.GetComponentInParent<ClosetHidingSystem>();

                if (closet != null && hit.collider.CompareTag("Closet"))
                {
                    bool canHide = true;

                    if (entity != null && !entity.canHideFromEnemy)
                    {
                        canHide = false;
                    }

                    if (whiteLady != null)
                    {
                        float distanceToWL = Vector3.Distance(transform.position, whiteLady.transform.position);

                        if (whiteLady.CurrentState == WhiteLady.State.Chasing && distanceToWL < safeHideDistance)
                        {
                            canHide = false;
                        }
                    }

                    if (!canHide)
                    {
                        Debug.Log("Enemy is too close. Cannot hide yet.");
                        return;
                    }

                    if (entity != null)
                    {
                        if (entityAi != null) entityAi.enabled = false;
                        if (entityWondering != null) entityWondering.enabled = true;
                    }

                    currentCloset = closet;
                    CanInteract = false;
                    isTransitioningToHide = true; // The AI instantly drops chase here!
                    StartCoroutine(InputDelay());
                    StartCoroutine(currentCloset.GoInsideCloset_CO());
                }
            }
        }
    }

    IEnumerator InputDelay()
    {
        inputLocked = true;
        yield return new WaitForSeconds(inputDelay);
        inputLocked = false;
    }
}