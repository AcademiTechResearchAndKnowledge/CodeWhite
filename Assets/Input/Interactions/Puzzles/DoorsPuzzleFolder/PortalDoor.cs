using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PortalDoor : MonoBehaviour
{
    [Header("Portal Setup")]
    public Camera portalCamera;
    public PortalDoor linkedDoor;
    public Transform player;
    public Material idlePortalMaterial;
    public Material portalMaterialBase;

    private Material portalRenderMat;

    [Header("Settings")]
    public float interactDistance = 10f;
    public float portalActiveTime = 5f;
    public float backtrackBlockTime = 3f;

    private bool portalActive = false;
    private bool recentlyUsed = false;
    private Renderer portalScreenRenderer;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        Transform screen = transform.Find("Portal_Mesh");
        if (screen != null)
            portalScreenRenderer = screen.GetComponent<Renderer>();

        if (portalMaterialBase != null)
            portalRenderMat = new Material(portalMaterialBase);

        if (portalScreenRenderer != null)
            portalScreenRenderer.material = idlePortalMaterial;

        if (portalCamera != null)
            portalCamera.enabled = false;

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }
    }

    void Update()
    {
        if (player == null || linkedDoor == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactDistance && !recentlyUsed)
        {
            if (Keyboard.current.fKey.wasPressedThisFrame)
                ActivatePortal();
        }
    }

    void LateUpdate()
    {
        if (!portalActive || portalCamera == null || linkedDoor == null || player == null)
            return;


        TransformThroughPortalCamera();
        portalCamera.enabled = true;
    }

    void TransformThroughPortalCamera()
    {
        if (Camera.main == null || linkedDoor == null) return;

        // Compute the relative position and rotation
        Vector3 relativePos = transform.InverseTransformPoint(Camera.main.transform.position);
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * Camera.main.transform.rotation;

        // Apply to linked portal
        portalCamera.transform.position = linkedDoor.transform.TransformPoint(relativePos);
        portalCamera.transform.rotation = linkedDoor.transform.rotation * relativeRot;


        portalCamera.transform.Rotate(0f, 180f, 0f);

        // Assign the portal texture
        if (portalScreenRenderer != null && portalMaterialBase != null)
        {
            portalScreenRenderer.material = new Material(portalMaterialBase);
            portalScreenRenderer.material.mainTexture = portalCamera.targetTexture;
        }
    }

    void ActivatePortal()
    {
        portalActive = true;
        StartCoroutine(AutoClosePortal(portalActiveTime));
    }

    IEnumerator AutoClosePortal(float delay)
    {
        yield return new WaitForSeconds(delay);

        portalActive = false;

        if (portalCamera != null)
            portalCamera.enabled = false;

        if (portalScreenRenderer != null)
            portalScreenRenderer.material = idlePortalMaterial;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!portalActive || recentlyUsed || linkedDoor == null) return;

        if (other.transform == player)
        {
            TeleportPlayer(player);

            linkedDoor.recentlyUsed = true;
            StartCoroutine(linkedDoor.ResetBacktrack(backtrackBlockTime));

            recentlyUsed = true;
        }
    }

    void TeleportPlayer(Transform player)
    {
        if (linkedDoor == null || player == null) return;


        Vector3 offset = player.position - transform.position;


        Vector3 forwardComponent = Vector3.Project(offset, transform.forward);
        Vector3 lateralComponent = offset - forwardComponent;

        float forwardDistance = forwardComponent.magnitude; 
        player.position = linkedDoor.transform.position + linkedDoor.transform.forward * forwardDistance + lateralComponent;


        float deltaY = linkedDoor.transform.eulerAngles.y - transform.eulerAngles.y + 180f;
        player.Rotate(0f, deltaY, 0f, Space.World);
    }

    IEnumerator ResetBacktrack(float delay)
    {
        yield return new WaitForSeconds(delay);
        recentlyUsed = false;
    }
}