using UnityEngine;
using Unity.Cinemachine;

public class PlayerReferences : MonoBehaviour
{
    public Rigidbody rb;
    public Collider playerCollider;
    public PlayerMovement movementScript;
    public Flashlight flashlightScript;
    public MeshRenderer bodyMeshRenderer;
    public CinemachineCamera playerCam;
    public PlayerLook playerLook;
}