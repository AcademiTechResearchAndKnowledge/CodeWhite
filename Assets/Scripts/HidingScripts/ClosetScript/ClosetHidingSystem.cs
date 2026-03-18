using UnityEngine;

public class ClosetHidingSystem : MonoBehaviour
{
    public GameObject VcamPlayer;
    public GameObject VcamCloset;

    public bool InsideCloset = false;

    public void GoInsideCloset()
    {
        VcamCloset.SetActive(true);
        VcamPlayer.SetActive(false);
        InsideCloset = true;
    }

    public void GoOutsideCloset()
    {
        VcamCloset.SetActive(false);
        VcamPlayer.SetActive(true);
    }
}
