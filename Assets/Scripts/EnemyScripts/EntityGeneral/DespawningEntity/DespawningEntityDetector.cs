using UnityEngine;

public class DespawningEntityDetector : MonoBehaviour
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

    private bool isChaseMusicPlaying = false;
    // REMOVED: isWaitingToStopChaseMusic

    private bool isCurrentlyIgnoringHiddenPlayer = false;
    private Vector3 lastKnownPlayerPosition;

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private TableHideState playerTableState;

    private ClosetHidingSystem[] allClosets;

    private AggroEntityAI entityAi;
    private DespawningEntityWondering entityWondering; // Updated reference

    void Awake()
    {
        entityAi = GetComponent<AggroEntityAI>();
        entityWondering = GetComponent<DespawningEntityWondering>(); // Updated reference

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        FindPlayerReferences();

        allClosets = FindObjectsByType<ClosetHidingSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        isCurrentlyIgnoringHiddenPlayer = false;

        // --- MODIFIED: Force instant chase on spawn ---
        isLookingPlayer = true;
        if (entityAi != null) entityAi.enabled = true;
        if (entityWondering != null) entityWondering.enabled = false;

        SetChaseMusic(true);
        Debug.Log("Despawning entity spawned! Instantly hunting the player.");
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
            Debug.LogError("DespawningEntityDetector: No object with tag 'Player' found.");
        }

        GameObject followObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (followObj != null)
        {
            playerTransform = followObj.transform;
        }
        else
        {
            Debug.LogError("DespawningEntityDetector: No object with tag 'PlayerFollow' found.");
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

        // ─── CONDITION 1: Player attempts to hide ─────────────────────────────────
        if (isHiding)
        {
            if (!isCurrentlyIgnoringHiddenPlayer)
            {
                // Check if the entity is too close when the hide attempt happens
                if (distanceToPlayer <= detectRange)
                {
                    // FAILED HIDE: The entity sees through the trick
                    isLookingPlayer = true;
                    Debug.Log("Player hid too close! Despawning entity is attacking!");
                }
                else
                {
                    // SUCCESSFUL HIDE: Safe distance
                    isCurrentlyIgnoringHiddenPlayer = true;
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
                        }
                        else
                        {
                            entityWondering.StartWanderingInstantly();
                            SetChaseMusic(false);
                        }
                    }
                }
            }

            // Only protect the player and return IF they successfully hid
            if (isCurrentlyIgnoringHiddenPlayer)
            {
                // Note: Since this entity despawns instead of relocating, the music will stop abruptly
                // when the gameObject is destroyed.
                return; // Safe! Skip the chase logic below.
            }
        }
        else
        {
            isCurrentlyIgnoringHiddenPlayer = false;
        }

        if (isLookingPlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }

        // ─── CONDITION 2: Player is within detect range (and not hiding) ─────────
        if (distanceToPlayer <= detectRange)
        {
            bool successfullySneaking = playerIsCrouching && distanceToPlayer > crouchSafeDistance;

            if (isLookingPlayer || !successfullySneaking)
            {
                isLookingPlayer = true;
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
            SetChaseMusic(false);

            entityAi.enabled = false;

            if (!entityWondering.enabled)
            {
                entityWondering.enabled = true;
            }
        }
    }

    private void SetChaseMusic(bool play)
    {
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