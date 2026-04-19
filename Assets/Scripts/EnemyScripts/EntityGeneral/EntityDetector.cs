using UnityEngine;

public class EntityDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 8f;
    public float hideAllowedRange = 12f;
    public float loseRange = 15f;
    public float crouchSafeDistance = 3f;

    [Tooltip("How far away from the player's last known position the entity should stop.")]
    public float investigateStopDistance = 5f;

    [Header("State")]
    public bool isLookingPlayer;
    public bool canHideFromEnemy;
    public float distanceToPlayer;

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private ClosetHideInteract playerHideInteract;
    private TableHideState playerTableState;
    private EntityAiOlder entityAi;
    private EntityWondering entityWondering;

    void Awake()
    {
        entityAi = GetComponent<EntityAiOlder>();
        entityWondering = GetComponent<EntityWondering>();
    }

    void Start()
    {
        FindPlayerReferences();
    }

    void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerHideInteract = playerObj.GetComponent<ClosetHideInteract>();
            playerTableState = playerObj.GetComponent<TableHideState>();

            PlayerReferences refs = playerObj.GetComponent<PlayerReferences>();
            if (refs != null)
            {
                playerMovement = refs.movementScript;
            }
        }
        else
        {
            Debug.LogError("No object with tag 'Player' found in scene.");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Player can hide only when farther than 12
        canHideFromEnemy = distanceToPlayer > hideAllowedRange;

        bool playerIsCrouching = playerMovement != null && playerMovement.isCrouching;

        // --- Determine if the player is currently hidden ---
        bool isHidingInCloset = playerHideInteract != null && playerHideInteract.IsHiding;
        bool isHidingUnderTable = playerTableState != null && playerTableState.isUnderTable && playerIsCrouching;

        // Automatically check if the player is in ANY closet OR crouching under a table
        if (isHidingInCloset || isHidingUnderTable)
        {
            // Check if we were JUST chasing the player before they hid
            if (isLookingPlayer)
            {
                // Tell the wondering script to go to the offset position instead of the exact position
                entityWondering.InvestigateLocation(GetOffsetInvestigatePosition());
            }

            isLookingPlayer = false;
            entityAi.enabled = false;
            entityWondering.enabled = true;
            return;
        }

        // Start chase if within detect range
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

        // Keep chasing until beyond lose range
        if (isLookingPlayer && distanceToPlayer <= loseRange)
        {
            entityAi.enabled = true;
            entityWondering.enabled = false;
            return;
        }

        // Stop chase if beyond lose range
        if (distanceToPlayer > loseRange)
        {
            if (isLookingPlayer)
            {
                // Tell the wondering script to go to the offset position
                entityWondering.InvestigateLocation(GetOffsetInvestigatePosition());
            }

            isLookingPlayer = false;
            entityAi.enabled = false;
            entityWondering.enabled = true;
        }
    }

    private Vector3 GetOffsetInvestigatePosition()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

        float travelDistance = Mathf.Max(0f, distanceToPlayer - investigateStopDistance);

        return transform.position + (directionToPlayer * travelDistance);
    }
}