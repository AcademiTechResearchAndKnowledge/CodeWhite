using UnityEngine;

public class PersistAcrossScenes : MonoBehaviour
{
    private static PersistAcrossScenes instance;

    private void Awake()
    {
        // If a persistent player already exists, destroy duplicates
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}