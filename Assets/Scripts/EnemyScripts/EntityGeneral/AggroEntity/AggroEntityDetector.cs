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
    public AudioSource audioSource; // For chase music
    public AudioSource ambientAudioSource; // For random entity noises
    public AudioClip chasingSfx;
    public AudioClip chasingStoppedSfx;

    [Header("Ambient Noise Settings")]
    [Tooltip("Add multiple clips for variety. The entity will pick one at random.")]
    public AudioClip[] ambientNoises;
    [Tooltip("Minimum time in seconds between random noises.")]
    public float minNoiseInterval = 4f;
    [Tooltip("Maximum time in seconds between random noises.")]
    public float maxNoiseInterval = 10f;
    [Tooltip("Volume for ambient noises.")]
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
            // Set chase music to 2D so it plays everywhere at maximum volume
            audioSource.spatialBlend = 0f;
        }
        if (ambientAudioSource != null)
        {
            // Keep ambient entity noises in 3D so the player can track them
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

        if (isLookingPlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }

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

        if (isLookingPlayer && distanceToPlayer <= loseRange)
        {
            entityAi.enabled = true;
            entityWondering.enabled = false;
            return;
        }

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