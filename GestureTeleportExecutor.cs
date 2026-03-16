using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GestureTeleportExecutor : MonoBehaviour
{
    public XRRayInteractor teleportRay;
    public TeleportationProvider teleportProvider;

    public void TeleportNow()
    {
        if (teleportRay.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            TeleportationArea area = hit.collider.GetComponent<TeleportationArea>();

            if (area != null)
            {
                TeleportRequest request = new TeleportRequest();
                request.destinationPosition = hit.point;
                request.destinationRotation = Quaternion.identity;

                teleportProvider.QueueTeleportRequest(request);
            }
        }
    }
}
