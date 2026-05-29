using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DDOLCleanup : MonoBehaviour
{
    [SerializeField] private float cleanupDelay = 5f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(CleanupAfterDelay());
    }

    private IEnumerator CleanupAfterDelay()
    {
        yield return new WaitForSeconds(cleanupDelay);
        CleanupDontDestroyOnLoad();
    }

    private void CleanupDontDestroyOnLoad()
    {
        GameObject temp = new GameObject("Temp-DDOL-Finder");
        DontDestroyOnLoad(temp);

        Scene dontDestroyScene = temp.scene;

        Destroy(temp);

        GameObject[] rootObjects = dontDestroyScene.GetRootGameObjects();

        foreach (GameObject obj in rootObjects)
        {
            if (obj == null) continue;

      
            if (IsUIObject(obj))
                continue;

            Destroy(obj);
        }
    }

    private bool IsUIObject(GameObject obj)
    {

        if (obj.GetComponent<Canvas>() != null)
            return true;

        if (obj.GetComponent<RectTransform>() != null)
            return true;

        if (obj.GetComponentInChildren<Canvas>(true) != null)
            return true;

        return false;
    }
}