using UnityEngine;

public class TRIGGERTESTETETST : MonoBehaviour
{
    [SerializeField] private RandomPortalSpawner RPS;

    private void Awake()
    {
        RPS = FindFirstObjectByType<RandomPortalSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger hit: " + other.name);

        if (!other.CompareTag("Player"))
        {
            Debug.Log("Not player");
            return;
        }

        if (RPS == null)
        {
            Debug.Log("RPS is NULL");
            return;
        }

        Debug.Log("Spawning portal");

        RPS.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Vertical);
    }
}