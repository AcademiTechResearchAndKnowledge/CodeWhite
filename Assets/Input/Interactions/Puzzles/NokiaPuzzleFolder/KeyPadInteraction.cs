using UnityEngine;

public class PhoneButton : MonoBehaviour
{
    public int number;
    public NokiaPuzzle puzzle;

    public void Interact()
    {
        Debug.Log("Pressed " + number);
        puzzle.PressNumber(number);
    }
}