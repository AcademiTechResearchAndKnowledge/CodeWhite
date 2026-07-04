using UnityEngine;

// This forces Unity to make sure a CanvasGroup is attached to this GameObject
[RequireComponent(typeof(CanvasGroup))]
public class PersistentUI : MonoBehaviour
{
    public static PersistentUI Instance { get; private set; }

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // Grab the Canvas Group
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Method to toggle the UI on and off
    public void SetUIVisibility(bool isVisible)
    {
        if (canvasGroup != null)
        {
            // alpha 0 makes it completely invisible, 1 makes it fully visible
            canvasGroup.alpha = isVisible ? 1f : 0f;

            // This stops the UI from blocking your mouse clicks while invisible
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
    }
}