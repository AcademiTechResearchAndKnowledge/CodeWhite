using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerTeleport : Interactable
{
    [Header("Teleport Settings")]
    public Transform[] teleportPoints;

    public float teleportInterval = 45f;

    public float minSpawnDistance = 10f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip teleportSound;

    private int currentIndex = -1;
    private bool collected = false;
    private Transform playerTransform;

    private void Start()
    {
        message = "Press F to collect flower";

        // Cache the player's transform to check distance later
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("FlowerTeleport: Could not find object with 'Player' tag.");
        }

        if (teleportPoints.Length > 0)
        {
            TeleportToValidPoint();
            StartCoroutine(TeleportRoutine());
        }
    }

    private IEnumerator TeleportRoutine()
    {
        while (!collected)
        {
            yield return new WaitForSeconds(teleportInterval);

            if (!collected)
            {
                TeleportToValidPoint();
            }
        }
    }

    private void TeleportToValidPoint()
    {
        if (teleportPoints.Length == 0) return;

        List<int> validIndices = new List<int>();

        // 1. Gather all points that are outside the minimum spawn distance
        for (int i = 0; i < teleportPoints.Length; i++)
        {
            if (i == currentIndex) continue;

            if (playerTransform != null)
            {
                float distanceToPlayer = Vector3.Distance(teleportPoints[i].position, playerTransform.position);

                if (distanceToPlayer >= minSpawnDistance)
                {
                    validIndices.Add(i);
                }
            }
            else
            {
                validIndices.Add(i);
            }
        }

        int newIndex = currentIndex;

        // 2. Pick a random point from the valid list
        if (validIndices.Count > 0)
        {
            newIndex = validIndices[Random.Range(0, validIndices.Count)];
        }
        // 3. Fallback: If the player happens to be too close to ALL points, default to standard random
        else
        {
            Debug.Log("FlowerTeleport: Player is too close to all points. Defaulting to standard random.");
            if (teleportPoints.Length > 1)
            {
                while (newIndex == currentIndex)
                {
                    newIndex = Random.Range(0, teleportPoints.Length);
                }
            }
            else
            {
                newIndex = 0;
            }
        }

        currentIndex = newIndex;

        // Apply new position and rotation
        transform.position = teleportPoints[currentIndex].position;
        transform.rotation = teleportPoints[currentIndex].rotation;

        if (audioSource != null && teleportSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(teleportSound);
        }
    }

    public override void Interact()
    {
        if (collected) return;

        base.Interact();

        collected = true;
        StopAllCoroutines();

        WLObjectiveManager.Instance.CollectFlower();

        DisableOutline();
        gameObject.SetActive(false);
    }
}