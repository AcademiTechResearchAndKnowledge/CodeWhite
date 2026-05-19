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

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip chasingSfx;
    public AudioClip chasingStoppedSfx;

    // --- NEW AUDIO TRACKING VARIABLES ---
    private bool isChaseMusicPlaying = false;
    private bool isWaitingToStopChaseMusic = false;

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

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        FindPlayerReferences();

        allClosets = FindObjectsByType<ClosetHidingSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        isLookingPlayer = false;
        isCurrentlyIgnoringHiddenPlayer = false;
        isChaseMusicPlaying = false;
        isWaitingToStopChaseMusic = false;

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
        bool isHiding = isHidingInCloset || isHidingUnderTable;

        // ─── CONDITION 1: Player attempts to hide ────────────────────────────────
        if (isHiding)
        {
            if (!isCurrentlyIgnoringHiddenPlayer)
            {
                // Check if the entity is too close when the hide attempt happens
                if (distanceToPlayer <= detectRange)
                {
                    // FAILED HIDE: The entity is 8 units or closer. It sees through the trick.
                    // We force the chase state here so it bypasses crouching/sneaking protections below.
                    isLookingPlayer = true;
                    Debug.Log("Player hid too close! Entity is attacking!");
                }
                else
                {
                    // SUCCESSFUL HIDE: Player was safely outside detectRange
                    isCurrentlyIgnoringHiddenPlayer = true;

                    // If they are > 8 units, they are only noticed if already being chased
                    bool didEntityNoticeHiding = isLookingPlayer;

                    isLookingPlayer = false;
                    if (entityAi != null) entityAi.enabled = false;

                    if (entityWondering != null)
                    {
                        entityWondering.enabled = true;

                        if (didEntityNoticeHiding)
                        {
                            lastKnownPlayerPosition = playerTransform.position;
                            entityWondering.InvestigateLocation(lastKnownPlayerPosition);
                            SetChaseMusic(true);
                            isWaitingToStopChaseMusic = true;
                        }
                        else
                        {
                            entityWondering.StartWanderingInstantly();
                            SetChaseMusic(false);
                            isWaitingToStopChaseMusic = false;
                        }
                    }
                }
            }

            // Only protect the player and return IF they successfully hid
            if (isCurrentlyIgnoringHiddenPlayer)
            {
                if (isWaitingToStopChaseMusic && entityWondering != null)
                {
                    if (entityWondering.currentState == AggroEntityWondering.WanderState.Relocating ||
                      entityWondering.currentState == AggroEntityWondering.WanderState.Normal)
                    {
                        isWaitingToStopChaseMusic = false;
                        SetChaseMusic(false);
                    }
                }
                return; // Safe! Skip the chase logic below.
            }
        }
        else
        {
            isCurrentlyIgnoringHiddenPlayer = false;
        }

        // Keep updating the last known position while actively chasing
        if (isLookingPlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }

        // ─── CONDITION 2: Player is within detect range (and not hidden safely) ──
        if (distanceToPlayer <= detectRange)
        {
            bool successfullySneaking = playerIsCrouching && distanceToPlayer > crouchSafeDistance;

            if (isLookingPlayer || !successfullySneaking)
            {
                isLookingPlayer = true;
                isWaitingToStopChaseMusic = false;
                SetChaseMusic(true);

                entityAi.enabled = true;
                entityWondering.enabled = false;
                return;
            }
        }

        // ─── CONDITION 3: Player is currently being chased, but hasn't escaped yet
        if (isLookingPlayer && distanceToPlayer <= loseRange)
        {
            entityAi.enabled = true;
            entityWondering.enabled = false;
            return;
        }

        // ─── CONDITION 4: Player ran far away (outside lose range) ───────────────
        if (distanceToPlayer > loseRange)
        {
            isLookingPlayer = false;
            isWaitingToStopChaseMusic = false;
            SetChaseMusic(false);

            entityAi.enabled = false;

            if (!entityWondering.enabled)
            {
                entityWondering.enabled = true;
            }
        }
    }

    /// <summary>
    /// Handles toggling the chase audio state independently of the AI behavior.
    /// </summary>
    private void SetChaseMusic(bool play)
    {
        // If we are already in the requested audio state, do nothing
        if (isChaseMusicPlaying == play) return;

        isChaseMusicPlaying = play;

        if (audioSource != null)
        {
            if (play)
            {
                if (chasingSfx != null)
                {
                    audioSource.clip = chasingSfx;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                audioSource.Stop();
                audioSource.clip = null;
                audioSource.loop = false;

                if (chasingStoppedSfx != null)
                {
                    audioSource.PlayOneShot(chasingStoppedSfx);
                }
            }
        }
    }
}