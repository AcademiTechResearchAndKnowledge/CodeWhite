using UnityEngine;

/// <summary>
/// White Lady exclusive detection module.
/// Pure data provider — answers questions about the player's state.
/// Does NOT toggle components or make state decisions.
/// </summary>
public class WhiteLadyDetection : MonoBehaviour
{
    // ─────────────────────────────────────────
    //  Detection Settings
    // ─────────────────────────────────────────
    [Header("Detection Settings")]
    [Tooltip("Distance at which the White Lady first sees the player.")]
    public float detectRange = 6f;

    [Tooltip("Distance at which she gives up the chase.")]
    public float loseRange = 10f;

    [Tooltip("Player must be farther than this to be allowed to hide.")]
    public float hideAllowedRange = 12f;

    [Tooltip("Crouching only works beyond this distance.")]
    public float crouchSafeDistance = 3f;

    [Tooltip("How far short of the player's last position she stops when investigating.")]
    public float investigateStopDistance = 5f;

    // ─────────────────────────────────────────
    //  Read-Only Info (visible in Inspector)
    // ─────────────────────────────────────────
    [Header("Info (Read-Only)")]
    public float distanceToPlayer;
    public bool  canHideFromEnemy;

    // ─────────────────────────────────────────
    //  Private — player references
    // ─────────────────────────────────────────
    private Transform          playerTransform;
    private PlayerMovement     playerMovement;
    private ClosetHideInteract playerClosetInteract;
    private TableHideState     playerTableState;

    // ─────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────
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

    // ─────────────────────────────────────────
    //  Public Queries — called by WhiteLady.cs
    // ─────────────────────────────────────────

    /// <summary>Returns true if the player is hidden in a closet or crouching under a table.</summary>
    public bool IsPlayerHiding()
    {
        bool inCloset   = playerClosetInteract != null && playerClosetInteract.IsHiding;
        bool underTable = playerTableState     != null && playerTableState.isUnderTable
                          && IsPlayerCrouching();
        return inCloset || underTable;
    }

    /// <summary>Returns true if the player is crouching far enough away to avoid detection.</summary>
    public bool IsPlayerSneakingSuccessfully()
    {
        return IsPlayerCrouching() && distanceToPlayer > crouchSafeDistance;
    }

    /// <summary>
    /// Returns the position she should walk toward when the player breaks line of sight.
    /// Stops short of the exact position by investigateStopDistance so it feels natural.
    /// </summary>
    public Vector3 GetLastKnownPosition()
    {
        if (playerTransform == null) return transform.position;

        Vector3 direction    = (playerTransform.position - transform.position).normalized;
        float   travelDist   = Mathf.Max(0f, distanceToPlayer - investigateStopDistance);
        return transform.position + direction * travelDist;
    }

    // ─────────────────────────────────────────
    //  Private Helpers
    // ─────────────────────────────────────────

    bool IsPlayerCrouching()
    {
        return playerMovement != null && playerMovement.isCrouching;
    }

    void ResolvePlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogError("[WhiteLadyDetection] No GameObject with tag 'Player' found in scene.");
            return;
        }

        playerTransform      = playerObj.transform;
        playerClosetInteract = playerObj.GetComponent<ClosetHideInteract>();
        playerTableState     = playerObj.GetComponent<TableHideState>();

        PlayerReferences refs = playerObj.GetComponent<PlayerReferences>();
        if (refs != null)
            playerMovement = refs.movementScript;
        else
            Debug.LogWarning("[WhiteLadyDetection] PlayerReferences not found — crouch detection disabled.");
    }
}
