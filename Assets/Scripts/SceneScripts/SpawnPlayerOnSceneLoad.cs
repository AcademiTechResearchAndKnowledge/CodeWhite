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

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = spawn.transform.position;
            rb.rotation = spawn.transform.rotation;
        }
        else
        {
            player.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
        }
    }
}