using UnityEngine;
using System.Collections.Generic;

public class SwitchPuzzleHandler : MonoBehaviour
{
    public static SwitchPuzzleHandler Instance;

    [Header("Correct Combination")]
    private List<int> correctCombination = new List<int>();

    [Header("Player Input")]
    public List<int> flippedSwitches = new List<int>();

    public GameObject lightFX;

    private bool isSolved = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateCombination();
    }

    void GenerateCombination()
    {
        correctCombination.Clear();

        while (correctCombination.Count < 2)
        {
            int rand = Random.Range(1, 4);
            if (!correctCombination.Contains(rand))
            {
                correctCombination.Add(rand);
            }
        }

        Debug.Log("Correct Combo: " + string.Join(", ", correctCombination));
    }

    public void FlipSwitch(int switchID)
    {
        if (isSolved) return; // puzzle is locked after submission

        if (!flippedSwitches.Contains(switchID))
        {
            flippedSwitches.Add(switchID);
        }
        else
        {
            flippedSwitches.Remove(switchID);
        }

        Debug.Log("Current flipped: " + string.Join(", ", flippedSwitches));

        // Update light as a preview
        if (lightFX != null)
            lightFX.SetActive(CheckCombination());
    }

    public bool CheckCombination()
    {
        if (flippedSwitches.Count != correctCombination.Count)
            return false;

        foreach (int sw in correctCombination)
        {
            if (!flippedSwitches.Contains(sw))
                return false;
        }

        return true;
    }

    // Called only by Submit button
    public void SubmitPuzzle()
    {
        if (isSolved)
        {
            Debug.Log("Puzzle already solved.");
            return;
        }

        if (CheckCombination())
        {
            isSolved = true;

            Debug.Log("Puzzle officially solved!");
            // Light is already on from preview
        }
        else
        {
            Debug.Log("Wrong combination submitted.");
        }
    }

    public bool IsSolved()
    {
        return isSolved;
    }
}