using UnityEngine;

public class OutOfBoundsTrigger : MonoBehaviour
{
    [SerializeField] private PlayerActionDetector detector;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        detector.TriggerOutOfBounds();
    }
}