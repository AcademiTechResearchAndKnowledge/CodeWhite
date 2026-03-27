using System.Collections;
using UnityEngine;

public class CandleInteract : Interactable
{
    [SerializeField] private Light torchLight;
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

        isLit = true;
        torchLight.enabled = true;
        onCandleLit?.Invoke();
        //flame.SetActive(true);

        LighterItem.hasLighter = false;

        PuzzleManager.instance.CandleLit();
    }
}