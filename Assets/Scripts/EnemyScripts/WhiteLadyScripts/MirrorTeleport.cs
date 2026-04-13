using System.Collections.Generic;
using UnityEngine;

public class MirrorPieceRandomSpawner : MonoBehaviour
{
    [Header("Mirror Spawn Settings")]
    [Tooltip("The base prefab that contains your MirrorPieceCollectible script and Collider")]
    public GameObject mirrorPiecePrefab;

    [Tooltip("Drag your 6 unique 3D models/assets here")]
    public GameObject[] uniqueVisualAssets;

    public Transform[] possibleSpawnPoints;
    public int numberOfPiecesToSpawn = 6;
    public float groundOffset = 0.5f;

    private void Start()
    {
        SpawnMirrorPieces();
    }

    private void SpawnMirrorPieces()
    {
        if (mirrorPiecePrefab == null || possibleSpawnPoints.Length == 0) return;

        if (uniqueVisualAssets.Length < numberOfPiecesToSpawn)
        {
            Debug.LogError("You don't have enough unique assets assigned to spawn without duplicates!");
            return;
        }

        List<Transform> availablePoints = new List<Transform>(possibleSpawnPoints);

        for (int i = 0; i < numberOfPiecesToSpawn; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform chosenPoint = availablePoints[randomIndex];
            Vector3 spawnPos = chosenPoint.position + new Vector3(0f, groundOffset, 0f);

            GameObject spawnedBase = Instantiate(
                mirrorPiecePrefab,
                spawnPos,
                Quaternion.identity,
                transform
            );
            spawnedBase.name = "SpawnedMirror_Piece_" + i;

            GameObject uniqueVisual = Instantiate(
                uniqueVisualAssets[i],
                spawnedBase.transform.position,
                spawnedBase.transform.rotation,
                spawnedBase.transform 
            );
            uniqueVisual.name = "UniqueVisualModel";

            availablePoints.RemoveAt(randomIndex);
        }
    }
}