using UnityEngine;

public class EntityDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 8f;
    public float hideAllowedRange = 12f;
    public float loseRange = 15f;
    public float crouchSafeDistance = 3f;

    [Header("State")]
    public bool isLookingPlayer;
    public bool canHideFromEnemy;
    public float distanceToPlayer;

    // Auto-Assigned References
    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private ClosetHideInteract playerHideInteract;
    private EntityAi entityAi;
    private EntityWondering entityWondering;

    void Awake()
    {
        entityAi = GetComponent<EntityAi>();
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

            // Utilize your existing PlayerReferences script!
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

        // Automatically check if the player is in ANY closet via their script
        if (playerHideInteract != null && playerHideInteract.IsHiding)
        {
            // --- NEW: Check if we were JUST chasing the player before they hid ---
            if (isLookingPlayer)
            {
                // Tell the wondering script to go to their last known position
                entityWondering.InvestigateLocation(playerTransform.position);
            }

            isLookingPlayer = false;
            entityAi.enabled = false;
            entityWondering.enabled = true;
            return;
        }

        bool playerIsCrouching = playerMovement != null && playerMovement.isCrouching;

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
            // --- NEW: Also investigate if the player just runs out of distance naturally! ---
            if (isLookingPlayer)
            {
                entityWondering.InvestigateLocation(playerTransform.position);
            }

            isLookingPlayer = false;
            entityAi.enabled = false;
            entityWondering.enabled = true;
        }
    }
}