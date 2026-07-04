using UnityEngine;

public class LibraryBook : MonoBehaviour
{
    public GameObject[] bookModels; // Your Red, Blue, Green, Orange models
    public int selectedVisualIndex = -1;

    private void Start()
    {
        // If the index hasn't been set by the spawner/inventory yet, pick a random one
        if (selectedVisualIndex == -1)
        {
            selectedVisualIndex = Random.Range(0, bookModels.Length);
        }

        UpdateVisuals();
    }

    // --- NEW: A public method to force the visuals to refresh ---
    public void UpdateVisuals()
    {
        // 1. Turn them all off first
        foreach (GameObject model in bookModels)
        {
            model.SetActive(false);
        }

        // 2. Turn on only the correct one
        if (selectedVisualIndex >= 0 && selectedVisualIndex < bookModels.Length)
        {
            bookModels[selectedVisualIndex].SetActive(true);
        }
    }
}