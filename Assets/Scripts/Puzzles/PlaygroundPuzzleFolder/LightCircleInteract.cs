using UnityEngine;

public class LightCircle : MonoBehaviour
{
    public LightCircleSpawner spawner;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Light triggered");

            if (spawner != null)
            {
                spawner.LightTriggered(gameObject);
            }
            else
            {
                Debug.LogError("Spawner reference is missing on LightCircle");
            }
        }
    }
}