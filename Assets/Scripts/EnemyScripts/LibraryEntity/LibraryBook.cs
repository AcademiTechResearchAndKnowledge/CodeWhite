using UnityEngine;

public class LibraryBook : MonoBehaviour
{
    [Header("Visual Randomization")]
    [Tooltip("Drag the EMPTY PARENT objects of your different 3D models here.")]
    public GameObject[] bookModels;

    // --- ADD THIS LINE to store the choice ---
    [HideInInspector] public int selectedVisualIndex = 0;

    private void Awake()
    {
        foreach (GameObject model in bookModels)
        {
            if (model != null) model.SetActive(false);
        }

        if (bookModels.Length > 0)
        {
            int randomIndex = Random.Range(0, bookModels.Length);

            // --- ADD THIS LINE to save the random number ---
            selectedVisualIndex = randomIndex;

            if (bookModels[randomIndex] != null)
            {
                bookModels[randomIndex].SetActive(true);
            }
        }
    }
}