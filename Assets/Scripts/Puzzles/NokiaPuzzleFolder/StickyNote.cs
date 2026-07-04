using UnityEngine;
using TMPro;

public class StickyNote : MonoBehaviour
{
    public TextMeshPro text;

    public void DisplayNumber(int[] numbers)
    {
        string result = "";

        foreach(int num in numbers)
        {
            result += num.ToString();
        }

        text.text = result;
    }
}