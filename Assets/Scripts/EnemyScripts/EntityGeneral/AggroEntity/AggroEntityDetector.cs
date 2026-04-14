using UnityEngine;

public class AggroEntityDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 8f;
    public float hideAllowedRange = 12f;
    public float loseRange = 15f;
    public float crouchSafeDistance = 3f;

    [Header("State")]
    public bool isLookingPlayer = false; // Start false, because we are investigating, not chasing yet
    public bool canHideFromEnemy;
    public float distanceToPlayer;

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private ClosetHideInteract playerHideInteract;
    private TableHideState playerTableState;

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

        // --- THE NEW SPAWN LOGIC ---
        // Instead of locking onto the player dynamically, 
        // we tell the entity to walk to the player's exact starting coordinate.
        isLookingPlayer = false;

        if (entityAi != null) entityAi.enabled = false;

        if (entityWondering != null && playerTransform != null)
        {
            entityWondering.enabled = true;
            // Force the entity to investigate the spot you were standing in when it spawned
            entityWondering.InvestigateLocation(playerTransform.position);
        }
    }

    void FindPlayerReferences()
    {
        // Grab hiding state scripts from the main Player object
        GameObject mainPlayerObj = GameObject.FindGameObjectWithTag("Player");
        if (mainPlayerObj != null)
        {
            playerHideInteract = mainPlayerObj.GetComponent<ClosetHideInteract>();
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

        // Grab physical tracking purely from the PlayerFollow tag
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

        bool isHidingInCloset = playerHideInteract != null && playerHideInteract.IsHiding;
        bool isHidingUnderTable = playerTableState != null && playerTableState.isUnderTable && playerIsCrouching;

        // Condition 1: Player manages to hide
        if (isHidingInCloset || isHidingUnderTable)
        {
            isLookingPlayer = false;
            entityAi.enabled = false;

            // Revert to wandering state if hidden
            if (!entityWondering.enabled)
            {
                entityWondering.enabled = true;
            }
            return;
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