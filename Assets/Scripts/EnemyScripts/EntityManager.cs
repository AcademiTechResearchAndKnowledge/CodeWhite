using UnityEngine;

public class EntityManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip WhispererWhisper;

    PuzzleManager puzzleManager;
    AudioSource audioSource;
    int whispererStage = 1;

    private void OnEnable()
    {
        CandleInteract.onCandleLit += TriggerWhisperer;
    }

    private void OnDisable()
    {
        CandleInteract.onCandleLit -= TriggerWhisperer;
    }

    private void Awake()
    {
        puzzleManager = GameObject.Find("PuzzleManager").GetComponent<PuzzleManager>();
        audioSource = GetComponent<AudioSource>();
    }

    bool whispererSpawned = false;

    void TriggerWhisperer()
    {
        Debug.Log("Checking Trigger: Whisperer");
        // NOTE: add a decrease chance right after despawning
        int triggerChance = puzzleManager.candlesLit;
        if (Random.Range(0, 10) < 10 && !whispererSpawned)
        {
            switch (whispererStage)
            {
                case 1:
                    // Play Whispering Audio
                    audioSource.clip = WhispererWhisper;
                    audioSource.Play();

                    Debug.Log("Whisperer whispers to you");

                    break;
                case 2:
                    // Flicker Lights
                    Debug.Log("Lights flicker around you");
                    break;
                case 3:
                    // Spawn Whisperer
                    Debug.Log("Whisperer is now here");
                    WhispererSpawner spawner = GameObject.Find("WhispererSpawner").GetComponent<WhispererSpawner>();

                    whispererSpawned = true;
                    spawner.SpawnWhisperer();
                    whispererStage = 0;
                    break;
            }

            whispererStage++;
        }
    }
}
