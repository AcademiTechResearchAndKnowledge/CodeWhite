using UnityEngine;

public class DrawerInteract : Interactable
{
    public Transform lighterSpawnPoint;

    // Remove the 'opened' flag or use it only for animation


    public override void Interact()
    {
        
        if (PuzzleManager.instance.currentDrawer == this)
        {
            PuzzleManager.instance.SpawnLighter(lighterSpawnPoint.position, lighterSpawnPoint.rotation);
        }

    
  
        Debug.Log("Drawer opened: " + gameObject.name);
    }
}