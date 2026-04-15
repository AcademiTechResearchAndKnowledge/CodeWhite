using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClosetHideInteract : MonoBehaviour
{
    public bool CanInteract = true;

    private EntityDetector entity;
    private EntityAi entityAi;
    private EntityWondering entityWondering;

    public float inputDelay = 2f;
    private bool inputLocked = false;

    private ClosetHidingSystem currentCloset;

    // Used by the EntityDetector to know if the player is currently inside
    public bool IsHiding => currentCloset != null && currentCloset.InsideCloset;

    void Start()
    {
        FindEntityReferences();
    }

    void FindEntityReferences()
    {
        // Using FindObjectOfType is much safer for prefabs! 
        // It doesn't rely on you remembering to set tags in the Inspector.
        entity = Object.FindFirstObjectByType<EntityDetector>();

        if (entity != null)
        {
            entityAi = entity.GetComponent<EntityAi>();
            entityWondering = entity.GetComponent<EntityWondering>();
        }
    }

    void Update()
    {
        if (inputLocked) return;

        // Keep trying to find the entity if it spawned late
        if (entity == null)
        {
            FindEntityReferences();
        }

        // --- EXIT CLOSET ---
        if (Keyboard.current.gKey.wasPressedThisFrame && currentCloset != null && currentCloset.InsideCloset)
        {
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
                    // === THE GATEKEEPER LOGIC ===
                    if (entity != null)
                    {
                        // 1. If the enemy's script says we cannot hide, STOP immediately.
                        if (!entity.canHideFromEnemy)
                        {
                            Debug.Log("Enemy is still too close. Cannot hide yet.");
                            return; // This kicks us out of the interaction!
                        }

                        // 2. If we passed the distance check, disable the enemy AI chasing
                        if (entityAi != null) entityAi.enabled = false;
                        if (entityWondering != null) entityWondering.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning("No Enemy found in scene, allowing hide by default.");
                    }

                    // === PROCEED TO HIDE ===
                    currentCloset = closet;
                    CanInteract = false;
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