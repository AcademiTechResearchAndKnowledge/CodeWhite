using UnityEngine;
using System.Collections;

public class EerieEasterEgg : MonoBehaviour
{
    [Header("Sprite References")]
    [Tooltip("Drag the GameObject containing your SpriteRenderer or UI Image here.")]
    public GameObject eerieSpriteObject;

    [Header("Timing Settings")]
    [Tooltip("Minimum time (in seconds) before the sprite MIGHT appear.")]
    public float minWaitTime = 15f;

    [Tooltip("Maximum time (in seconds) before the sprite WILL appear.")]
    public float maxWaitTime = 45f;

    [Tooltip("How long the sprite stays visible (in seconds). Keep it low for a quick flash!")]
    public float flashDuration = 0.15f;

    void Start()
    {
        // Make sure the sprite is hidden when the main menu first loads
        if (eerieSpriteObject != null)
        {
            eerieSpriteObject.SetActive(false);

            // Start the infinite loop of waiting and flashing
            StartCoroutine(EerieFlashRoutine());
        }
        else
        {
            Debug.LogWarning("EerieEasterEgg script is missing a reference to the sprite object!");
        }
    }

    IEnumerator EerieFlashRoutine()
    {
        // This while(true) loop will run forever as long as the main menu is active
        while (true)
        {
            // 1. Pick a random wait time between your minimum and maximum
            float waitTime = Random.Range(minWaitTime, maxWaitTime);

            // 2. Wait for that amount of time in the background
            yield return new WaitForSeconds(waitTime);

            // 3. Spook the player! Turn the sprite ON.
            eerieSpriteObject.SetActive(true);

            // 4. Wait for just a split second
            yield return new WaitForSeconds(flashDuration);

            // 5. Hide the sprite again. The loop will now restart.
            eerieSpriteObject.SetActive(false);
        }
    }
}