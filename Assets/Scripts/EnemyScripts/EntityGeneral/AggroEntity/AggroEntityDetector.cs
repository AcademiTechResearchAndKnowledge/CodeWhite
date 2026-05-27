using UnityEngine;

public class AggroEntityDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectRange = 8f;
    public float hideAllowedRange = 12f;
    public float loseRange = 15f;
    public float crouchSafeDistance = 3f;

    // [NEW] Line of Sight & Verticality Settings
    [Header("Line of Sight Settings")]
    public LayerMask obstacleLayer;
    [Tooltip("Max vertical distance allowed to spot the player (e.g., one floor height).")]
    public float maxHeightDifference = 3.5f;
    [Tooltip("How long the entity will keep chasing you after breaking line of sight.")]
    public float loseSightDelay = 2.0f;

    private float currentLoseTimer; // Tracks how long the player has been out of sight

    [Header("State")]
    public bool isLookingPlayer = false;
    public bool canHideFromEnemy;
    public float distanceToPlayer; // Note: Now calculates 2D flat distance

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioSource ambientAudioSource;
    public AudioClip chasingSfx;
    public AudioClip chasingStoppedSfx;

    [Header("Ambient Noise Settings")]
    public AudioClip[] ambientNoises;
    public float minNoiseInterval = 4f;
    public float maxNoiseInterval = 10f;
    [Range(0f, 1f)] public float ambientVolume = 0.8f;

    private float noiseTimer;
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

        if (audioSource != null)
        {
            audioSource.spatialBlend = 0f;
        }
        if (ambientAudioSource != null)
        {
            ambientAudioSource.spatialBlend = 1f;
        }
    }

    void Start()
    {
        FindPlayerReferences();
        allClosets = FindObjectsByType<ClosetHidingSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        isLookingPlayer = false;
        isCurrentlyIgnoringHiddenPlayer = false;
        isChaseMusicPlaying = false;
        isWaitingToStopChaseMusic = false;
        currentLoseTimer = loseSightDelay; // [NEW] Initialize timer

        ResetNoiseTimer();

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
        HandleAmbientNoises();

        if (playerTransform == null) return;

        // [NEW] Calculate 2D distance on the X and Z axes only to prevent floor-hacking
        Vector3 entityPos2D = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 playerPos2D = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
        distanceToPlayer = Vector3.Distance(entityPos2D, playerPos2D);

        // [NEW] Check actual height difference. If already chasing, ignore this rule so it can follow up stairs.
        float heightDifference = Mathf.Abs(transform.position.y - playerTransform.position.y);
        bool isOnSameFloor = (heightDifference <= maxHeightDifference) || isLookingPlayer;

        // [NEW] Check Line of Sight
        bool hasLineOfSight = HasLineOfSight();

        canHideFromEnemy = distanceToPlayer > hideAllowedRange;
        bool playerIsCrouching = playerMovement != null && playerMovement.isCrouching;

        // --- Hiding Logic (Kept Original) ---
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

        if (isHiding)
        {
            if (!isCurrentlyIgnoringHiddenPlayer)
            {
                if (distanceToPlayer <= detectRange)
                {
                    isLookingPlayer = true;
                    Debug.Log("Player hid too close! Entity is attacking!");
                }
                else
                {
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
                return;
            }
        }
        else
        {
            isCurrentlyIgnoringHiddenPlayer = false;
        }

        // --- Detection & Chase Logic ---
        if (isLookingPlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }

        // 1. Initial Spotting
        if (distanceToPlayer <= detectRange && !isLookingPlayer)
        {
            bool successfullySneaking = playerIsCrouching && distanceToPlayer > crouchSafeDistance;

            // [NEW] Entity must be on the same floor AND have Line of Sight to spot you
            if (!successfullySneaking && isOnSameFloor && hasLineOfSight)
            {
                isLookingPlayer = true;
                isWaitingToStopChaseMusic = false;
                SetChaseMusic(true);

                entityAi.enabled = true;
                entityWondering.enabled = false;
            }
        }

        // 2. Maintaining the Chase
        if (isLookingPlayer)
        {
            // [NEW] Cooldown logic for losing line of sight
            if (hasLineOfSight)
            {
                currentLoseTimer = loseSightDelay; // Reset timer if the monster can see you
            }
            else
            {
                currentLoseTimer -= Time.deltaTime; // Tick down if you are behind cover
            }

            // Keep chasing if close enough AND timer hasn't run out
            if (distanceToPlayer <= loseRange && currentLoseTimer > 0f)
            {
                entityAi.enabled = true;
                entityWondering.enabled = false;
                return;
            }
        }

        // 3. Losing the Player
        // [NEW] Modified to drop aggro if distance is too far OR the timer ran out
        if (isLookingPlayer && (distanceToPlayer > loseRange || currentLoseTimer <= 0f))
        {
            isLookingPlayer = false;
            isWaitingToStopChaseMusic = false;
            SetChaseMusic(false);

            entityAi.enabled = false;

            if (!entityWondering.enabled)
            {
                entityWondering.enabled = true;
                // Go to the last place the player was seen before breaking line of sight
                entityWondering.InvestigateLocation(lastKnownPlayerPosition);
            }
        }
    }

    // [NEW] Helper Method for Line of Sight
    private bool HasLineOfSight()
    {
        if (playerTransform == null) return false;

        // Offset positions up by 1.5 units so the raycast fires from chest/eye level, not from the floor
        Vector3 startPos = transform.position + (Vector3.up * 1.5f);
        Vector3 endPos = playerTransform.position + (Vector3.up * 1.5f);

        Vector3 directionToPlayer = endPos - startPos;
        float distance = directionToPlayer.magnitude;

        // Fire the Raycast
        if (Physics.Raycast(startPos, directionToPlayer.normalized, out RaycastHit hit, distance, obstacleLayer))
        {
            // Ray hit a wall, floor, or obstacle
            return false;
        }

        // Clear line of sight
        return true;
    }

    private void HandleAmbientNoises()
    {
        if (!isChaseMusicPlaying && ambientNoises != null && ambientNoises.Length > 0)
        {
            noiseTimer -= Time.deltaTime;

            if (noiseTimer <= 0f)
            {
                PlayRandomAmbientNoise();
                ResetNoiseTimer();
            }
        }
    }

    private void PlayRandomAmbientNoise()
    {
        if (ambientAudioSource != null && !ambientAudioSource.isPlaying)
        {
            AudioClip randomClip = ambientNoises[Random.Range(0, ambientNoises.Length)];
            ambientAudioSource.clip = randomClip;
            ambientAudioSource.volume = ambientVolume;
            ambientAudioSource.Play();
        }
    }

    private void ResetNoiseTimer()
    {
        noiseTimer = Random.Range(minNoiseInterval, maxNoiseInterval);
    }

    private void SetChaseMusic(bool play)
    {
        if (isChaseMusicPlaying == play) return;

        isChaseMusicPlaying = play;

        if (audioSource != null)
        {
            if (play)
            {
                if (ambientAudioSource != null) ambientAudioSource.Stop();

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