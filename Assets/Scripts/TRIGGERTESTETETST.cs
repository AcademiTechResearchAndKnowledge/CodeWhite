using UnityEngine;

public class TRIGGERTESTETETST : MonoBehaviour
{
    public RandomPortalSpawner RPS;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        RPS.SpawnPortalRandom();
        
    }
}
