using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StalkerFollowScript : MonoBehaviour
{
    private NavMeshAgent agent;
    private PlayerReferences playerRefs;
    private float movementThreshold;

    private enum SpawnReason { Idle, Closet }
    private SpawnReason spawnReason;

    [Header("Canvas Jumpscare Setup")]
    [Tooltip("Drag the GameObject with your JumpscareMechanic script here.")]
    [SerializeField] private JumpscareMechanic canvasJumpscare;

    [Tooltip("How close the stalker needs to get to the target to trigger the scare.")]
    [SerializeField] private float attackDistance = 1.0f;

    [Tooltip("How long the stalker stares at you BEFORE the scare plays (Applies to Closet only now).")]
    [SerializeField] private float stareDelay = 1.0f;

    [Tooltip("Amount of anxiety to add to the player.")]
    [SerializeField] private float anxietyPenalty = 30f;

    [Header("VFX")]
    [Tooltip("The particle effect to spawn when the entity disappears.")]
    [SerializeField] private GameObject despawnParticlePrefab;

    private bool isJumpscaring = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (canvasJumpscare == null)
        {
            canvasJumpscare = FindAnyObjectByType<JumpscareMechanic>();
        }
    }

    public void InitializeForIdle(PlayerReferences refs, float threshold)
    {
        playerRefs = refs;
        movementThreshold = threshold;
        spawnReason = SpawnReason.Idle;
        UpdateDestination();
    }

    public void InitializeForCloset(PlayerReferences refs)
    {
        playerRefs = refs;
        spawnReason = SpawnReason.Closet;
        UpdateDestination();
    }

    private void Update()
    {
        if (isJumpscaring) return;

        UpdateDestination();
        CheckJumpscareTrigger();
        CheckDespawnConditions();
    }

    private void UpdateDestination()
    {
        if (spawnReason == SpawnReason.Closet)
        {
            ClosetHidingSystem currentCloset = ClosetHidingSystem.ActiveCloset;
            if (currentCloset != null && currentCloset.InsideCloset && currentCloset.stalkerFollowTarget != null)
            {
                agent.SetDestination(currentCloset.stalkerFollowTarget.transform.position);
                return;
            }
        }

        if (playerRefs != null)
        {
            agent.SetDestination(playerRefs.transform.position);
        }
    }

    private void CheckJumpscareTrigger()
    {
        if (spawnReason == SpawnReason.Closet)
        {
            ClosetHidingSystem currentCloset = ClosetHidingSystem.ActiveCloset;
            if (currentCloset != null && currentCloset.InsideCloset && currentCloset.stalkerFollowTarget != null)
            {
                Vector3 myFlatPos = new Vector3(transform.position.x, 0f, transform.position.z);
                Vector3 targetFlatPos = new Vector3(currentCloset.stalkerFollowTarget.transform.position.x, 0f, currentCloset.stalkerFollowTarget.transform.position.z);

                if (Vector3.Distance(myFlatPos, targetFlatPos) <= attackDistance)
                {
                    StartCoroutine(ClosetJumpscareRoutine(currentCloset));
                }
            }
        }
        else if (spawnReason == SpawnReason.Idle && playerRefs != null)
        {
            Vector3 myFlatPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 playerFlatPos = new Vector3(playerRefs.transform.position.x, 0f, playerRefs.transform.position.z);

            if (Vector3.Distance(myFlatPos, playerFlatPos) <= attackDistance)
            {
                StartCoroutine(IdleJumpscareRoutine());
            }
        }
    }

    private IEnumerator ClosetJumpscareRoutine(ClosetHidingSystem targetCloset)
    {
        isJumpscaring = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // 1. Shut down the sprite controller so it stops calculating directions
        SpriteDirectionalController dirController = GetComponentInChildren<SpriteDirectionalController>();
        if (dirController != null) dirController.enabled = false;

        // ---> NEW FIX: FORCE THE SPRITE TO FACE FORWARD <---
        // Grab the animator and manually force the floats to the "Front" facing position
        Animator stalkerAnim = GetComponentInChildren<Animator>();
        if (stalkerAnim != null)
        {
            stalkerAnim.SetFloat("moveX", 0f);
            stalkerAnim.SetFloat("moveY", 1f);
            stalkerAnim.SetBool("isWalking", false); // Force it into the Idle state
        }

        // Un-flip the sprite just in case it was walking to the left when it froze
        SpriteRenderer stalkerSprite = GetComponentInChildren<SpriteRenderer>();
        if (stalkerSprite != null)
        {
            stalkerSprite.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        // ----------------------------------------------------

        Transform targetTransform = targetCloset.stalkerFollowTarget.transform;
        transform.position = new Vector3(targetTransform.position.x, transform.position.y, targetTransform.position.z);
        transform.rotation = targetTransform.rotation;

        targetCloset.ForceOpenDoorsForJumpscare();

        yield return new WaitForSeconds(stareDelay);

        float waitTime = 2.0f;
        if (canvasJumpscare != null)
        {
            canvasJumpscare.TriggerJumpscare();
            waitTime = canvasJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(waitTime);

        if (stalkerSprite != null) stalkerSprite.enabled = false;

        try
        {
            if (targetCloset != null) targetCloset.ForceExitCloset();

            AnxietyHandler anxietyHandler = FindAnyObjectByType<AnxietyHandler>();
            if (anxietyHandler != null) anxietyHandler.AddAnxiety(anxietyPenalty);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error caught during Closet kick-out: " + e.Message);
        }

        Despawn(false);
    }

    private IEnumerator IdleJumpscareRoutine()
    {
        isJumpscaring = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        SpriteDirectionalController dirController = GetComponentInChildren<SpriteDirectionalController>();
        if (dirController != null) dirController.enabled = false;

        Vector3 lookPos = playerRefs.transform.position - transform.position;
        lookPos.y = 0f;
        if (lookPos != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookPos);
        }

        float waitTime = 2.0f;
        if (canvasJumpscare != null)
        {
            canvasJumpscare.TriggerJumpscare();
            waitTime = canvasJumpscare.animationDuration - 0.5f;
        }

        yield return new WaitForSeconds(waitTime);

        SpriteRenderer stalkerSprite = GetComponentInChildren<SpriteRenderer>();
        if (stalkerSprite != null) stalkerSprite.enabled = false;

        try
        {
            AnxietyHandler anxietyHandler = FindAnyObjectByType<AnxietyHandler>();
            if (anxietyHandler != null) anxietyHandler.AddAnxiety(anxietyPenalty);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error caught during Idle Jumpscare: " + e.Message);
        }

        // FALSE means: Do NOT play the particle effect!
        Despawn(false);
    }

    private void CheckDespawnConditions()
    {
        if (isJumpscaring) return;

        if (spawnReason == SpawnReason.Idle)
        {
            if (playerRefs != null && playerRefs.rb != null)
            {
                Vector3 flatVelocity = new Vector3(playerRefs.rb.linearVelocity.x, 0f, playerRefs.rb.linearVelocity.z);

                if (flatVelocity.magnitude > movementThreshold)
                {
                    // TRUE means: Player ran away! Play the particles!
                    Despawn(true);
                }
            }
        }
        else if (spawnReason == SpawnReason.Closet)
        {
            ClosetHidingSystem currentCloset = ClosetHidingSystem.ActiveCloset;
            if (currentCloset == null || !currentCloset.InsideCloset)
            {
                // TRUE means: Player left early! Play the particles!
                Despawn(true);
            }
        }
    }

    // UPDATED: Added the boolean toggle to control the VFX
    private void Despawn(bool showParticles)
    {
        if (showParticles && despawnParticlePrefab != null)
        {
            Instantiate(despawnParticlePrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}