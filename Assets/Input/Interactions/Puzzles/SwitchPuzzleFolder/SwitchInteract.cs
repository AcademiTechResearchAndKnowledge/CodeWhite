using UnityEngine;

public class SwitchInteract : MonoBehaviour
{
    public int switchID;

    public void FlipSwitch()
    {
        SwitchPuzzleHandler.Instance.FlipSwitch(switchID);
    }

    void OnMouseDown()
    {
        FlipSwitch();
    }
}