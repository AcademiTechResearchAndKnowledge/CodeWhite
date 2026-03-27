using UnityEngine;

public class CandleInteract : Interactable
{
    public GameObject flame;
    private bool isLit = false;

    public override void Interact()
    {
        if (isLit) return;

        if (!LighterItem.hasLighter)
        {
            Debug.Log("You need a lighter!");
            return;
        }

        isLit = true;
        flame.SetActive(true);

        LighterItem.hasLighter = false;

        PuzzleManager.instance.CandleLit();
    }
}