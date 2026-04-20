using UnityEngine;

public class CandleInteracted : Interactable
{
    public GameObject flame;
    private bool isLit = false;

    public override void Interact()
    {
        if (isLit) return;

        if (!LighterPickup.hasLighter)
        {
            Debug.Log("You need a lighter!");
            return;
        }

        isLit = true;
        flame.SetActive(true);

        LighterPickup.hasLighter = false;

        PuzzleManager.instance.CandleLit();
    }
}