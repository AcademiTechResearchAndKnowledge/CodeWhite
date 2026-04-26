using UnityEngine;

public class WhiteLadySubmitInteract : Interactable
{
    private WhiteLady whiteLady;

    // REMOVED: private Outline outline;
    // REMOVED: The Awake() method

    void Start()
    {
        whiteLady = GetComponentInParent<WhiteLady>();

        // We can just call the method from your base Interactable class 
        // to force the outline off at the very start!
        DisableOutline();
    }

    public override void Interact()
    {
        if (whiteLady == null) return;

        if (whiteLady.CurrentState != WhiteLady.State.Weeping)
        {
            Debug.Log("She is too hostile right now! You can only approach her when she is weeping.");
            return;
        }

        bool hasFlower = WLObjectiveManager.Instance.flowerCollected;
        bool hasFixedMirror = WLObjectiveManager.Instance.collectedMirrorPieces >= WLObjectiveManager.Instance.totalMirrorPieces;

        if (hasFlower || hasFixedMirror)
        {
            base.Interact();

            Debug.Log("Successfully submitted the objective items to the White Lady!");

            Destroy(gameObject);
        }
        else
        {
            Debug.Log("You don't have the Fixed Mirror or the Flower yet to submit!");
        }
    }
}