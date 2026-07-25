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

        if (RPS == null)
        {
            Debug.Log("RPS is NULL");
            return;
        }

        Debug.Log("Spawning portal");
        RPS.SpawnPortalRandom(RandomPortalSpawner.PortalOrientation.Horizontal);
    }
}