using UnityEngine;

/// <summary>
/// White Lady exclusive detection module.
/// Pure data provider — answers questions about the player's state.
/// Does NOT toggle components or make state decisions.
/// </summary>
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

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private ClosetHideInteract playerClosetInteract;
    private TableHideState playerTableState;

    void Start()
    {
        ResolvePlayerReferences();
    }

    void Update()
    {
        if (playerTransform == null) return;

        distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        canHideFromEnemy = distanceToPlayer > hideAllowedRange;
    }

    public bool HasLineOfSight()
    {
        if (playerTransform == null) return false;

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

    public bool IsPlayerHiding()
    {
        bool inCloset = playerClosetInteract != null && playerClosetInteract.IsHiding;
        bool underTable = playerTableState != null && playerTableState.isUnderTable
                          && IsPlayerCrouching();
        return inCloset || underTable;
    }

    public bool IsPlayerSneakingSuccessfully()
    {
        return IsPlayerCrouching() && distanceToPlayer > crouchSafeDistance;
    }

    public Vector3 GetLastKnownPosition()
    {
        if (playerTransform == null) return transform.position;

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float travelDist = Mathf.Max(0f, distanceToPlayer - investigateStopDistance);
        return transform.position + direction * travelDist;
    }

    bool IsPlayerCrouching()
    {
        return playerMovement != null && playerMovement.isCrouching;
    }

    void ResolvePlayerReferences()
    {
        // Now fully bulletproof: Uses Unity 6 search instead of relying on specific tags!
        PlayerReferences refs = FindAnyObjectByType<PlayerReferences>();
        if (refs != null)
        {
            playerTransform = refs.transform;
            playerMovement = refs.movementScript;
        }
        else
        {
            Debug.LogWarning("[WhiteLadyDetection] PlayerReferences not found.");
        }

        playerClosetInteract = FindAnyObjectByType<ClosetHideInteract>();
        playerTableState = FindAnyObjectByType<TableHideState>();

        if (playerClosetInteract == null)
            Debug.LogWarning("[WhiteLadyDetection] ClosetHideInteract script not found in scene!");
    }
}