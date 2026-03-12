using UnityEngine;

public class CallButton : MonoBehaviour
{
    public NokiaPuzzle puzzle;

    public void Interact()
    {
        puzzle.CallNumber();
    }
}