using UnityEngine;

public class TriggerRelay : MonoBehaviour
{
    [SerializeField] private PlayerActionDetector detector;
    public int area_CODE;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        detector.OnTriggered(other, area_CODE);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        detector.OnExitArea(area_CODE);
    }
}