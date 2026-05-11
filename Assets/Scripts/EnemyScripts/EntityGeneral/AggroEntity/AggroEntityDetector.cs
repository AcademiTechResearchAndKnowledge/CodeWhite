using UnityEngine;

public class AggroEntityDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 8f;
    public float hideAllowedRange = 12f;
    public float loseRange = 15f;
    public float crouchSafeDistance = 3f;

    [Header("State")]
    public bool isLookingPlayer = false;
    public bool canHideFromEnemy;
    public float distanceToPlayer;

    private bool isCurrentlyIgnoringHiddenPlayer = false;
    private Vector3 lastKnownPlayerPosition;

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private TableHideState playerTableState;

    private ClosetHidingSystem[] allClosets;

    private AggroEntityAI entityAi;
    private AggroEntityWondering entityWondering;

    void Awake()
    {
        entityAi = GetComponent<AggroEntityAI>();
        entityWondering = GetComponent<AggroEntityWondering>();
    }

    void Start()
    {
        FindPlayerReferences();

        allClosets = FindObjectsByType<ClosetHidingSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        isLookingPlayer = false;
        isCurrentlyIgnoringHiddenPlayer = false;

        if (entityAi != null) entityAi.enabled = false;

        if (entityWondering != null && playerTransform != null)
        {
            entityWondering.enabled = true;
            entityWondering.InvestigateLocation(playerTransform.position);
        }
    }

    void FindPlayerReferences()
    {
        GameObject mainPlayerObj = GameObject.FindGameObjectWithTag("Player");
        if (mainPlayerObj != null)
        {
            playerTableState = mainPlayerObj.GetComponent<TableHideState>();

            PlayerReferences refs = mainPlayerObj.GetComponent<PlayerReferences>();
            if (refs != null)
            {
                playerMovement = refs.movementScript;
            }
        }
        else
        {
            Debug.LogError("AggroEntityDetector: No object with tag 'Player' found.");
        }

        GameObject followObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (followObj != null)
        {
            playerTransform = followObj.transform;
        }
        else
        {
            Debug.LogError("AggroEntityDetector: No object with tag 'PlayerFollow' found.");
            if (mainPlayerObj != null) playerTransform = mainPlayerObj.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        canHideFromEnemy = distanceToPlayer > hideAllowedRange;

        bool playerIsCrouching = playerMovement != null && playerMovement.isCrouching;

        bool isHidingInCloset = false;
        if (allClosets != null)
        {
            foreach (var closet in allClosets)
            {
                if (closet != null && closet.InsideCloset)
                {
                    isHidingInCloset = true;
                    break;
                }
            }
        }

        bool isHidingUnderTable = playerTableState != null && playerTableState.isUnderTable && playerIsCrouching;

        // „Ÿ„Ÿ„Ÿ CONDITION 1: Player manages to hide „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ
        if (isHidingInCloset || isHidingUnderTable)
        {
            if (!isCurrentlyIgnoringHiddenPlayer)
            {
                isCurrentlyIgnoringHiddenPlayer = true;

                // --- ADDED LOGIC: Did the entity actually notice you hide? ---
                // It notices if it was already actively chasing you, OR if it happens to be close enough to see/hear you enter.
                bool didEntityNoticeHiding = isLookingPlayer || (distanceToPlayer <= detectRange);

                // Instantly disable jumpscare eligibility
                isLookingPlayer = false;

                if (entityAi != null) entityAi.enabled = false;

                if (entityWondering != null)
                {
                    entityWondering.enabled = true;

                    if (didEntityNoticeHiding)
                    {
                        // The monster saw you hide! Walk to the door to investigate.
                        lastKnownPlayerPosition = playerTransform.position;
                        entityWondering.InvestigateLocation(lastKnownPlayerPosition);
                        Debug.Log("Player hid while detected! Entity is investigating the last known position.");
                    }
                    else
                    {
                        // The monster was far away and completely oblivious. Keep wandering normally.
                        entityWondering.StartWanderingInstantly();
                        Debug.Log("Player hid secretly. Entity is oblivious and continues normal wandering.");
                    }
                }
            }
            return;
        }
        else
        {
            // Reset the flag when the player leaves the hiding spot
            isCurrentlyIgnoringHiddenPlayer = false;
        }

        // Keep updating the last known position while actively chasing
        if (isLookingPlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }

        // Condition 2: Player is within detect range (and not hiding) -> The entity spots you!
        if (distanceToPlayer <= detectRange)
        {
            bool successfullySneaking = playerIsCrouching && distanceToPlayer > crouchSafeDistance;

            if (isLookingPlayer || !successfullySneaking)
            {
                isLookingPlayer = true;
                entityAi.enabled = true;
                entityWondering.enabled = false;
                return;
            }
        }

        // Condition 3: Player is currently being chased, but hasn't escaped yet
        if (isLookingPlayer && distanceToPlayer <= loseRange)
        {
            entityAi.enabled = true;
            entityWondering.enabled = false;
            return;
        }

        // Condition 4: Player ran far away (outside lose range)
        if (distanceToPlayer > loseRange)
        {
            isLookingPlayer = false;
            entityAi.enabled = false;

            if (!entityWondering.enabled)
            {
                entityWondering.enabled = true;
            }
        }
    }
}