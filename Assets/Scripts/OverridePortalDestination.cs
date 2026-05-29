using UnityEngine;

public class HardcodedPortalTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    [Header("Portal")]
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Override Destination")]
    [SerializeField] private string targetScene;

    [Header("Portal Direction")]
    [SerializeField] private PortalNextStage.PortalOrientation orientation =
        PortalNextStage.PortalOrientation.Vertical;

    [Header("Surface Detection")]
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private LayerMask ceilingMask;

    [SerializeField] private float wallRayDistance = 5f;
    [SerializeField] private float ceilingRayDistance = 5f;
    [SerializeField] private float surfaceOffset = 0.05f;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered && triggerOnce) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        if (portalPrefab == null) return;

        Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;

        Vector3 spawnPos = basePos;
        Quaternion rot = Quaternion.identity;

        bool found = false;

        if (orientation == PortalNextStage.PortalOrientation.Horizontal)
        {
            Vector3[] dirs = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };

            foreach (Vector3 dir in dirs)
            {
                if (Physics.Raycast(basePos + Vector3.up * 0.5f, dir, out RaycastHit hit, wallRayDistance, wallMask))
                {
                    spawnPos = hit.point + hit.normal * surfaceOffset;
                    rot = BuildRotation(hit.normal);
                    found = true;
                    break;
                }
            }
        }
        else
        {
            if (Physics.Raycast(basePos, Vector3.up, out RaycastHit hit, ceilingRayDistance, ceilingMask))
            {
                spawnPos = hit.point + hit.normal * surfaceOffset;
                rot = BuildRotation(hit.normal);
                found = true;
            }
        }

        if (!found)
        {
            rot = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;
        }

        GameObject portalObj = Instantiate(portalPrefab, spawnPos, rot);

        PortalNextStage portal = portalObj.GetComponent<PortalNextStage>();

        if (portal != null)
        {
            portal.SetOrientation(orientation);

            if (!string.IsNullOrEmpty(targetScene))
                portal.SetForcedScene(targetScene);
        }
    }

    private Quaternion BuildRotation(Vector3 normal)
    {
        Vector3 up = normal;

        Vector3 forward = Vector3.Cross(Vector3.up, up);
        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.Cross(Vector3.forward, up);

        forward.Normalize();

        return Quaternion.LookRotation(forward, up);
    }
}