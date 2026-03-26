using UnityEngine;

public class ClosetHidingSystem : MonoBehaviour
{
    public GameObject VcamPlayer;
    public GameObject VcamCloset;

    public Transform player;
    public Transform exitPoint;
    public Rigidbody rb;

    public bool InsideCloset = false;

    public void GoInsideCloset()
    {
        VcamCloset.SetActive(true);
        VcamPlayer.SetActive(false);
        InsideCloset = true;
    }

    public void GoOutsideCloset()
    {

        player.position = exitPoint.position;
        player.rotation = exitPoint.rotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        VcamCloset.SetActive(false);
        VcamPlayer.SetActive(true);
        InsideCloset = false;
    }
}
