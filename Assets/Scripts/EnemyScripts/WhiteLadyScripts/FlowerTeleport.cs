using System.Collections;
using UnityEngine;

public class FlowerTeleport : Interactable
{
    [Header("Teleport Settings")]
    public Transform[] teleportPoints;
    public float teleportInterval = 120f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip teleportSound;

    private int currentIndex = -1;
    private bool collected = false;

    private void Start()
    {
        message = "Press F to collect flower";

        if (teleportPoints.Length > 0)
        {
            TeleportToRandomPoint();
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
                TeleportToRandomPoint();
            }
        }
    }

    private void TeleportToRandomPoint()
    {
        if (teleportPoints.Length == 0) return;

        int newIndex = currentIndex;

        if (teleportPoints.Length == 1)
        {
            newIndex = 0;
        }
        else
        {
            while (newIndex == currentIndex)
            {
                newIndex = Random.Range(0, teleportPoints.Length);
            }
        }

        currentIndex = newIndex;

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