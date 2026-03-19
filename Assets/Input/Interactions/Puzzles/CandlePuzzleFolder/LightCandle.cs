using UnityEngine;

public class CandleInteract : Interactable
{
    public delegate void OnCandleLit();
    public static event OnCandleLit onCandleLit;
    
    //public GameObject flame;
    private bool isLit = false;

    public override void Interact()
    {
        if (isLit) return;

        if (!LighterItem.hasLighter)
        {
            Debug.Log("You need a lighter!");
            return;
        }

        onCandleLit?.Invoke();
        isLit = true;
        //flame.SetActive(true);

        LighterItem.hasLighter = false;

        PuzzleManager.instance.CandleLit();
    }
}