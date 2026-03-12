using UnityEngine;

public class SwitchPuzzleHandler : MonoBehaviour
{

    public static SwitchPuzzleHandler Instance;
   
    [Header("Correct Switch (Randomized)")]
    private int correctSwitch;

    [Header("Player Switch")]
    private int flippedSwitch = -1;



    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Randomly choose the correct switch (1, 2, or 3)
        correctSwitch = Random.Range(1, 4);

        Debug.Log("Correct Switch: " + correctSwitch);
    }

    public void FlipSwitch(int switchID)
    {
        // Only allow one switch to be flipped
        if (flippedSwitch != -1)
        {
            Debug.Log("A switch has already been flipped.");
            return;
        }

        flippedSwitch = switchID;

        Debug.Log("Player flipped switch: " + switchID);
    }

    public void SubmitAnswer(int answer)
    {
        if (answer == correctSwitch)
        {
            Debug.Log("Correct switch!");

        }
        else
        {
            Debug.Log("Wrong switch!");

        }
    }
}