using UnityEngine;

public class CandleInteract : Interactable
{
    public delegate void OnCandleLit();
    public static event OnCandleLit onCandleLit;

    public GameObject flame;
    private bool isLit = false;

    public override void Interact()
    {
        Debug.Log("TRACKER: Player clicked the candle!");

        if (isLit) return;

        // Print exactly what the Manager thinks is happening right now
        Debug.Log("TRACKER: Candle is checking Manager... State is currently: " + LighterPuzzleManager.instance.currentLighterState);

        if (LighterPuzzleManager.instance.currentLighterState != LighterPuzzleManager.LighterState.Held)
        {
            Debug.Log("You need a lighter!");
            return;
        }

        isLit = true;
        flame.SetActive(true);
        onCandleLit?.Invoke();

        // THE FIX: We now pass 'this' specific candle script to the manager so it can keep track of it
        LighterPuzzleManager.instance.CandleLit(this);
    }

    // --- NEW: Method to turn the light and flame off ---
    public void Extinguish()
    {
        if (!isLit) return;

        isLit = false;
        flame.SetActive(false);
        Debug.Log("TRACKER: A candle was extinguished!");
    }
}