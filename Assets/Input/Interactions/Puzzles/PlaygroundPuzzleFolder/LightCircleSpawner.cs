using UnityEngine;
using System.Collections;

public class LightCircleSpawner : MonoBehaviour
{
    public GameObject lightCirclePrefab;
    public Transform[] spawnPoints;

    public float spawnInterval = 60f;

    void Start()
    {
        StartCoroutine(SpawnLights());
    }

    IEnumerator SpawnLights()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(lightCirclePrefab, point.position, Quaternion.identity);

            Debug.Log("Light circle appeared at: " + point.position);
        }
    }
}