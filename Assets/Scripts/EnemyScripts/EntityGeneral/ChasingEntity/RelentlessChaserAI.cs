using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RelentlessChaserAI : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip chasingSfx;

    private Transform playerTransform;
    private NavMeshAgent navMeshAgent;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        FindPlayerTarget();
        StartChaseMusic();
    }

    private void FindPlayerTarget()
    {
        // Prioritize the PlayerFollow target if you use one for center-mass tracking
        GameObject followObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (followObj != null)
        {
            playerTransform = followObj.transform;
        }
        else
        {
            // Fallback to the main player object
            GameObject mainPlayerObj = GameObject.FindGameObjectWithTag("Player");
            if (mainPlayerObj != null)
            {
                playerTransform = mainPlayerObj.transform;
            }
            else
            {
                Debug.LogError("RelentlessChaserAI: Could not find Player or PlayerFollow tags.");
            }
        }
    }

    private void StartChaseMusic()
    {
        if (audioSource != null && chasingSfx != null)
        {
            audioSource.clip = chasingSfx;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        // Unrelentingly track the player's exact position every frame
        if (playerTransform != null && navMeshAgent.enabled && !navMeshAgent.isStopped)
        {
            navMeshAgent.destination = playerTransform.position;
        }
    }
}