using TMPro;
using UnityEngine;

public class SubmitAnswerButton : MonoBehaviour
{
    public TMP_InputField inputField; // Drag your TMP Input Field here

    public void Submit()
    {
        // Convert the input field text to an int safely
        if (int.TryParse(inputField.text, out int answerID))
        {
            SwitchPuzzleHandler.Instance.SubmitAnswer(answerID);
        }
        else
        {
            Debug.Log("Invalid input! Please enter a number.");
        }
    }
}