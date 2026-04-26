using UnityEngine;
using System.Collections;

public class LightCircleSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject lightCirclePrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 60f;
    public Transform spawnArea;

    [Header("Puzzle Settings")]
    public int lightsNeeded = 5;

    private int lightsCompleted = 0;
    private GameObject currentLight;

    public RandomPortalSpawner RPS;

    private bool puzzleComplete = false;

    void Start()
    {
        StartCoroutine(SpawnLights());
    }

    IEnumerator SpawnLights()
    {
        while (true)
        {
            if (puzzleComplete)
                yield break;

            yield return new WaitForSeconds(spawnInterval);

            if (puzzleComplete)
                yield break;

            SpawnLight();
        }
    }

    void SpawnLight()
    {
        if (puzzleComplete) return;

        if (lightCirclePrefab == null)
        {
            Debug.LogError("lightCirclePrefab is not assigned");
            return;
        }

        if (spawnArea == null)
        {
            Debug.LogError("spawnArea is not assigned");
            return;
        }

        if (currentLight != null)
        {
            Destroy(currentLight);
            currentLight = null;
        }

        Vector3 randomPosition = GetRandomPosition();

        currentLight = Instantiate(lightCirclePrefab, randomPosition, Quaternion.identity);

        LightCircle light = currentLight.GetComponentInChildren<LightCircle>();

        if (light == null)
        {
            Debug.LogError("LightCircle script is missing on prefab");
            return;
        }

        light.spawner = this;
    }

    public void LightTriggered(GameObject light)
    {
        if (puzzleComplete) return;

        lightsCompleted++;

        Debug.Log("Light completed: " + lightsCompleted + " / " + lightsNeeded);

        Destroy(light);
        currentLight = null;

        if (lightsCompleted >= lightsNeeded)
        {
            CompletePuzzle();
        }
    }

    void CompletePuzzle()
    {
        if (puzzleComplete) return;

        puzzleComplete = true;

        if (currentLight != null)
        {
            Destroy(currentLight);
            currentLight = null;
        }

        Debug.Log("Puzzle complete");

        if (RPS != null)
        {
            RPS.SpawnPortalRandom();
        }
        else
        {
            Debug.LogError("RandomPortalSpawner (RPS) is not assigned");
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector3 size = spawnArea.GetComponent<Renderer>().bounds.size;
        Vector3 center = spawnArea.position;

        float x = Random.Range(center.x - size.x / 2, center.x + size.x / 2);
        float z = Random.Range(center.z - size.z / 2, center.z + size.z / 2);
        float y = center.y;

        return new Vector3(x, y, z);
    }

    void OnDrawGizmosSelected()
    {
        if (spawnArea == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            spawnArea.position,
            spawnArea.GetComponent<Renderer>().bounds.size
        );
    }
}