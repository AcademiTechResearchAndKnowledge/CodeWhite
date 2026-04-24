using UnityEngine;

public class CandleJumpscareTrigger : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("How close the entity needs to be to catch the player.")]
    public float catchRadius = 2.5f;

    private Transform playerTransform;
    private bool hasCaughtPlayer = false;

    private AggroEntityDetector entityDetector;

    private void Start()
    {
        entityDetector = GetComponent<AggroEntityDetector>();

        if (entityDetector == null)
        {
            Debug.LogError("AggroJumpscareTrigger: Cannot find AggroEntityDetector script on this entity!");
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("PlayerFollow");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("AggroJumpscareTrigger: No object with tag 'PlayerFollow' found in the scene.");
        }
    }

    private void Update()
    {
        if (playerTransform == null || hasCaughtPlayer || entityDetector == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= catchRadius && entityDetector.isLookingPlayer)
        {
            TriggerJumpscare();
        }
    }

    private void TriggerJumpscare()
    {
        hasCaughtPlayer = true;

        // 1. Placeholder for the Anxiety System
        Debug.Log("[Anxiety System] Player caught! Anxiety increased by 25.");

        // 2. Jumpscare Effects
        Debug.Log("[Jumpscare System] Jumpscare sequence initiated.");

        // 3. PENALTY: Tell the manager to blow out a candle and hide the lighter
        if (LighterPuzzleManager.instance != null)
        {
            LighterPuzzleManager.instance.BlowOutCandle();
        }

        // 4. Entity Disappears
        // Since you're using standard instantiation, this completely removes the entity from the scene.
        // The LighterPuzzleManager will automatically spawn a fresh one the next time a candle is lit.
        Debug.Log("[Entity Action] Entity is now disappearing.");
        Destroy(gameObject);
    }
}