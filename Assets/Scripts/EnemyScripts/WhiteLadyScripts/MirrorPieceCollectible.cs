using UnityEngine;

public class MirrorPieceCollectible : Interactable
{
    private bool collected = false;

    private void Start()
    {
        message = "Press F to collect mirror piece";
    }

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