using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PortalDoor : MonoBehaviour
{
    public Camera portalCamera;
    public PortalDoor linkedDoor;
    public Transform player;

    public Material idlePortalMaterial;
    public Material portalMaterialBase;

    public DoorController linkedDoorController;

    public float interactDistance = 10f;
    public float portalActiveTime = 5f;
    public float backtrackBlockTime = 1.5f;


    public float linkedDoorOpenTime = 3f;

    bool portalActive;
    bool recentlyUsed;

    Material runtimeMat;
    Renderer portalRenderer;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        Transform screen = transform.Find("Portal_Mesh");
        if (screen != null)
            portalRenderer = screen.GetComponent<Renderer>();

        if (portalMaterialBase != null)
            runtimeMat = new Material(portalMaterialBase);

        if (portalRenderer != null)
            portalRenderer.material = idlePortalMaterial;

        if (portalCamera != null)
            portalCamera.enabled = false;

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }

        if (linkedDoor != null)
            linkedDoorController = linkedDoor.GetComponentInParent<DoorController>();
    }

    void Update()
    {
        if (player == null || linkedDoor == null) return;

        float dist = Vector3.Distance(player.position, transform.position);

        if (dist <= interactDistance && !recentlyUsed)
        {
            if (Keyboard.current != null &&
                Keyboard.current.fKey.wasPressedThisFrame &&
                !portalActive)
            {
                ActivatePortal();
            }
        }
    }

    void LateUpdate()
    {
        if (!portalActive || portalCamera == null || linkedDoor == null || player == null)
            return;

        UpdatePortalCamera();
        portalCamera.enabled = true;
    }

    void UpdatePortalCamera()
    {
        if (Camera.main == null) return;

        Vector3 pos = transform.InverseTransformPoint(Camera.main.transform.position);
        Quaternion rot = Quaternion.Inverse(transform.rotation) * Camera.main.transform.rotation;

        portalCamera.transform.position =
            linkedDoor.transform.TransformPoint(pos);

        portalCamera.transform.rotation =
            linkedDoor.transform.rotation * rot;

        portalCamera.transform.Rotate(0f, 180f, 0f);

        if (portalRenderer != null && runtimeMat != null)
        {
            runtimeMat.mainTexture = portalCamera.targetTexture;
            portalRenderer.material = runtimeMat;
        }
    }

    void ActivatePortal()
    {
        portalActive = true;


        if (linkedDoorController != null)
        {
            linkedDoorController.OpenFromPortal();
            StartCoroutine(CloseLinkedDoorLater());
        }

        StartCoroutine(AutoClosePortal());
    }

    IEnumerator AutoClosePortal()
    {
        yield return new WaitForSeconds(portalActiveTime);

        portalActive = false;

        if (portalCamera != null)
            portalCamera.enabled = false;

        if (portalRenderer != null)
            portalRenderer.material = idlePortalMaterial;

        if (linkedDoorController != null)
            linkedDoorController.ForceCloseFromPortal();
    }

    IEnumerator CloseLinkedDoorLater()
    {
        yield return new WaitForSeconds(linkedDoorOpenTime);

        if (linkedDoorController != null)
        {
            linkedDoorController.ForceCloseFromPortal();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!portalActive || recentlyUsed || linkedDoor == null) return;

        if (other.transform == player)
        {
            Teleport(player);

            recentlyUsed = true;
            linkedDoor.recentlyUsed = true;

            StartCoroutine(ResetBacktrack());
            StartCoroutine(linkedDoor.ResetBacktrack());
        }
    }

    void Teleport(Transform t)
    {
        Vector3 offset = t.position - transform.position;

        Vector3 forward = Vector3.Project(offset, transform.forward);
        Vector3 lateral = offset - forward;

        float dist = forward.magnitude;

        t.position =
            linkedDoor.transform.position +
            linkedDoor.transform.forward * dist +
            lateral;

        float rot = linkedDoor.transform.eulerAngles.y
                  - transform.eulerAngles.y
                  + 180f;

        t.Rotate(0f, rot, 0f, Space.World);
    }

    IEnumerator ResetBacktrack()
    {
        yield return new WaitForSeconds(backtrackBlockTime);
        recentlyUsed = false;
    }
}