using UnityEngine;

public class SimpleCandleInteract : Interactable
{
    public delegate void OnSimpleCandleLit();
    public static event OnSimpleCandleLit onSimpleCandleLit;

    public GameObject flame;
    private bool isLit = false;

    public override void Interact()
    {

        isLit = !isLit;

        flame.SetActive(isLit);

        if (isLit == true)
        {
            onSimpleCandleLit?.Invoke();
        }
        else
        {
        }
    }
}