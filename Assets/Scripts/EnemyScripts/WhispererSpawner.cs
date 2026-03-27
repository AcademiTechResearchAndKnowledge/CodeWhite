using UnityEngine;

public class WhispererSpawner : MonoBehaviour
{
    public GameObject Whisperer;

    public void SpawnWhisperer()
    {
        // Spawn in a random predetermined area (location of its children)
        Transform spawner = transform.GetChild(Random.Range(0, transform.childCount));

        Whisperer.transform.position = spawner.position;
        Whisperer.SetActive(true);

    }

    public void DespawnWhisperer()
    {
        Whisperer.SetActive(false);
    }

}
