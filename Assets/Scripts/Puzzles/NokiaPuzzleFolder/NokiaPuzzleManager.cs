using UnityEngine;
using System.Collections.Generic;

public class NokiaPuzzle : MonoBehaviour
{
    public RandomPortalSpawner RPS;
    public int numberLength = 4;

    public int[] correctNumber;
    [SerializeField] private List<int> playerInput = new List<int>();

    public StickyNote note;

    private bool isSolved = false;

    void Start()
    {
        GenerateNumber();
        note.DisplayNumber(correctNumber);
    }

    void GenerateNumber()
    {
        correctNumber = new int[numberLength];

        for (int i = 0; i < numberLength; i++)
        {
            correctNumber[i] = Random.Range(0, 10);
        }
    }

    private float lastInputTime;
    public float inputDelay = 0.2f;

    public void PressNumber(int num)
    {
        if (isSolved)
            return;

        if (Time.time - lastInputTime < inputDelay)
            return;

        lastInputTime = Time.time;

        playerInput.Add(num);
        Debug.Log("Pressed: " + num);
    }

    public void CallNumber()
    {
        if (isSolved)
            return;

        if (playerInput.Count != correctNumber.Length)
        {
            WrongNumber();
            return;
        }

        for (int i = 0; i < correctNumber.Length; i++)
        {
            if (playerInput[i] != correctNumber[i])
            {
                WrongNumber();
                return;
            }
        }

        Success();
    }

    void WrongNumber()
    {
        Debug.Log("Wrong number! the entity speeds up");
        playerInput.Clear();
    }

    void Success()
    {
        isSolved = true;
        RPS.SpawnPortalRandom();
        Debug.Log("Correct number!");

 
        foreach (PhoneButton button in Object.FindObjectsByType<PhoneButton>(FindObjectsSortMode.None))
        {
            button.enabled = false;
        }
    }
}