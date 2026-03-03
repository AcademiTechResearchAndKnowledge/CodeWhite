using UnityEngine;

public class PortalNextStage : MonoBehaviour
{
    [Tooltip("Only objects with this tag can trigger the portal.")]
    public string playerTag = "Player";

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;

        if (RunManager.Instance == null)
        {
            Debug.LogError("PortalNextStage: No RunManager in the game. Add RunManager to your first scene.");
            return;
        }

        RunManager.Instance.LoadNextStage();
    }
}