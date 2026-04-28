using UnityEngine;
using Unity.Cinemachine;

public class PlayerCameraReference : MonoBehaviour
{
    public static CinemachineCamera Instance;

    private void Awake()
    {
        if (Instance != null && Instance != GetComponent<CinemachineCamera>())
        {
            Destroy(gameObject);
            return;
        }

        Instance = GetComponent<CinemachineCamera>();
        DontDestroyOnLoad(gameObject);
    }
}