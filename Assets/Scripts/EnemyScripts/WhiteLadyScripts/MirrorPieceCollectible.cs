using UnityEngine;

public class MirrorPieceCollectible : Interactable
{
    private bool collected = false;

    public override void Interact()
    {
        if (collected) return;

        base.Interact();

        collected = true;
        WLObjectiveManager.Instance.CollectMirrorPiece();

        DisableOutline();
        gameObject.SetActive(false);
    }
}