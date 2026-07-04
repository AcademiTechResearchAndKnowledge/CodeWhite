using UnityEngine;

public class WhiteLadyDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 6f;
    public float loseRange = 10f;
    public float hideAllowedRange = 12f;
    public float crouchSafeDistance = 3f;
    public float investigateStopDistance = 5f;

    [Header("Line of Sight")]
    public LayerMask obstacleMask;
    public float eyeHeight = 1.5f;
    public float playerCenterHeight = 1.0f;

    [Header("Info (Read-Only)")]
    public float distanceToPlayer;
    public bool canHideFromEnemy;
    public bool isLookingPlayer = false;

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private TableHideState playerTableState;
    private ClosetHidingSystem[] allClosets;
    private ClosetHideInteract playerClosetInteract;

    void Start()
    {
        ResolvePlayerReferences();
    }

    void Update()
    {
        // Continuous failsafe reference grab
        if (playerTransform == null || allClosets == null || allClosets.Length == 0)
        {
            ResolvePlayerReferences();
        }

        if (playerTransform == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Unified absolute check
        bool isCurrentlyHiding = IsPlayerHiding();

        // 1. Aggro Detection Logic
        if (!isLookingPlayer)
        {
            // Must NOT be hiding to gain aggro
            if (distanceToPlayer <= detectRange && !isCurrentlyHiding && HasLineOfSight())
            {
                if (!IsPlayerSneakingSuccessfully())
                {
                    isLookingPlayer = true;
                }
            }
        }
        else
        {
            // --- CRITICAL: Forces aggro off the moment hiding registers ---
            if (distanceToPlayer > loseRange || isCurrentlyHiding)
            {
                isLookingPlayer = false;
            }
        }

        // 2. Hide Condition
        canHideFromEnemy = !isLookingPlayer || (distanceToPlayer > hideAllowedRange);
    }

    public bool HasLineOfSight()
    {
        // Instantly kill the raycast if hidden
        if (playerTransform == null || IsPlayerHiding()) return false;

        Vector3 startPos = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPos = playerTransform.position + Vector3.up * playerCenterHeight;

        Vector3 direction = (targetPos - startPos).normalized;
        float distance = Vector3.Distance(startPos, targetPos);

        Debug.DrawRay(startPos, direction * distance, Color.yellow);

        RaycastHit[] hits = Physics.RaycastAll(startPos, direction, distance, obstacleMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hits.Length; i++)
        {
            Transform hitObj = hits[i].transform;
            if (hitObj.root == transform.root) continue;
            if (hitObj.root == playerTransform.root || hitObj.CompareTag("Player")) continue;
            return false;
        }

        return true;
    }

    // ─── UNIFIED SOURCE OF TRUTH FOR HIDING ───
    public bool IsPlayerHiding()
    {
        // 1. Primary check: Player movement script disabled by closet prop
        if (playerMovement != null && !playerMovement.enabled) return true;

        // 2. Secondary check: Active Closet prop arrays
        if (allClosets != null)
        {
            foreach (var closet in allClosets)
            {
                if (closet != null && closet.InsideCloset) return true;
            }
        }

        // 3. Fallback checks
        if (playerClosetInteract != null && playerClosetInteract.IsHiding) return true;
        if (playerTableState != null && playerTableState.isUnderTable && IsPlayerCrouching()) return true;

        return false;
    }

    public bool IsPlayerSneakingSuccessfully()
    {
        return IsPlayerCrouching() && distanceToPlayer > crouchSafeDistance;
    }

    public Vector3 GetLastKnownPosition()
    {
        if (IsPlayerHiding())
        {
            GameObject followObj = GameObject.FindGameObjectWithTag("PlayerFollow");
            if (followObj != null) return followObj.transform.position;
        }

        if (playerTransform == null) return transform.position;

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float travelDist = Mathf.Max(0f, distanceToPlayer - investigateStopDistance);
        return transform.position + direction * travelDist;
    }

    private bool IsPlayerCrouching()
    {
        return playerMovement != null && playerMovement.isCrouching;
    }

    private void ResolvePlayerReferences()
    {
        GameObject mainPlayer = GameObject.FindGameObjectWithTag("Player");
        if (mainPlayer != null)
        {
            playerTransform = mainPlayer.transform;
            playerMovement = mainPlayer.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                PlayerReferences refs = mainPlayer.GetComponent<PlayerReferences>();
                if (refs != null) playerMovement = refs.movementScript;
            }

            playerTableState = mainPlayer.GetComponent<TableHideState>();
            playerClosetInteract = mainPlayer.GetComponent<ClosetHideInteract>();
        }

        allClosets = FindObjectsByType<ClosetHidingSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
}