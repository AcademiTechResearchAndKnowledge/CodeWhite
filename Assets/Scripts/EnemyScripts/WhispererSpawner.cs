using UnityEngine;

public class WhispererSpawner : MonoBehaviour
{
    public GameObject Whisperer;

    public void SpawnWhisperer()
    {
        // Spawn in a random predetermined area (location of its children)
        Transform spawner = transform.GetChild(Random.Range(0, transform.childCount));

        Instantiate(Whisperer, spawner.position, Quaternion.identity);
    }
}
