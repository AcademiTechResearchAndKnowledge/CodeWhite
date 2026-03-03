using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnPlayerOnSceneLoad : MonoBehaviour
{
    public string playerTag = "Player";
    public string spawnTag = "PlayerSpawn";

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        GameObject spawn = GameObject.FindGameObjectWithTag(spawnTag);
        if (spawn == null) return;

        player.transform.position = spawn.transform.position;
        player.transform.rotation = spawn.transform.rotation;
    }
}