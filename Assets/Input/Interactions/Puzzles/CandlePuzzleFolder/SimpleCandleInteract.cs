using UnityEngine;

public class SimpleCandleInteract : Interactable
{
    public delegate void OnSimpleCandleLit();
    public static event OnSimpleCandleLit onSimpleCandleLit;

    public GameObject flame;
    private bool isLit = false;

    public override void Interact()
    {
        Debug.Log("TRACKER: Player clicked the simple candle!");

        isLit = !isLit;

        flame.SetActive(isLit);

        if (isLit == true)
        {
            onSimpleCandleLit?.Invoke();
            Debug.Log("Simple Candle Lit!");
        }
        else
        {
            Debug.Log("Simple Candle Extinguished!");
        }
    }
}