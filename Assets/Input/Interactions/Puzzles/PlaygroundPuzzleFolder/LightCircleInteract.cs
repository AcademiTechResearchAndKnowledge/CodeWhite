using UnityEngine;

public class LightCircle : MonoBehaviour
{
    public float lifeTime = 10f; // how long the light stays

    void Start()
    {
        Debug.Log("Light circle appeared.");

        Destroy(gameObject, lifeTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player ran through the light circle!");

            Destroy(gameObject);
        }
    }
}