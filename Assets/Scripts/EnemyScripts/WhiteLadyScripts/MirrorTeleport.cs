using System.Collections.Generic;
using UnityEngine;

public class MirrorPieceRandomSpawner : MonoBehaviour
{
    [Header("Mirror Spawn Settings")]
    public GameObject mirrorPiecePrefab;
    public Transform[] possibleSpawnPoints;
    public int numberOfPiecesToSpawn = 6;
    public float groundOffset = 0.5f;

    private void Start()
    {
        SpawnMirrorPieces();
    }

    private void SpawnMirrorPieces()
    {
        if (mirrorPiecePrefab == null || possibleSpawnPoints.Length == 0)
            return;

        List<Transform> availablePoints = new List<Transform>(possibleSpawnPoints);

        for (int i = 0; i < numberOfPiecesToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform chosenPoint = availablePoints[randomIndex];

            Vector3 spawnPos = chosenPoint.position + new Vector3(0f, groundOffset, 0f);

            GameObject spawned = Instantiate(
                mirrorPiecePrefab,
                spawnPos,
                Quaternion.identity,
                transform
            );

            spawned.name = "SpawnedMirror_" + i;

            availablePoints.RemoveAt(randomIndex);
        }
    }
}