using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CHURentity : MonoBehaviour
{
    [SerializeField] public Transform movePositionTransform;
    public PlayerMovement player_MOVEMENT;
    private NavMeshAgent navMeshAgent;

    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.5f;

    private bool isJumping = false;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.autoTraverseOffMeshLink = false;
        player_MOVEMENT = GameObject.Find("Player").GetComponent<PlayerMovement>();
        movePositionTransform = GameObject.Find("Player").transform;
    }

    private void Update()
    {
        if (!player_MOVEMENT.isCrouching && movePositionTransform)
        {
            navMeshAgent.destination = movePositionTransform.position;
        }

        if (navMeshAgent.isOnOffMeshLink && !isJumping)
        {
            StartCoroutine(JumpAcrossLink());
        }
    }

    private IEnumerator JumpAcrossLink()
    {
        isJumping = true;
        navMeshAgent.isStopped = true;

        OffMeshLinkData data = navMeshAgent.currentOffMeshLinkData;

        Vector3 startPos = transform.position;
        Vector3 endPos = data.endPos + Vector3.up * navMeshAgent.baseOffset;

        float time = 0f;

        while (time < jumpDuration)
        {
            float t = time / jumpDuration;
            float height = 4 * jumpHeight * t * (1 - t);
            transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            time += Time.deltaTime;
            yield return null;
        }

        navMeshAgent.Warp(endPos);
        navMeshAgent.CompleteOffMeshLink();
        navMeshAgent.isStopped = false;
        navMeshAgent.ResetPath();

        isJumping = false;
    }
}