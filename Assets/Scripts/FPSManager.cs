using UnityEngine;


public class FPSManager : MonoBehaviour
{
    public int frameRateLimit = 60;

    void Awake()
    {
        // 0 = Don't sync, 1 = Sync with screen refresh rate
        QualitySettings.vSyncCount = 0; 
        
        // Set your target FPS
        Application.targetFrameRate = frameRateLimit;
    }
}

