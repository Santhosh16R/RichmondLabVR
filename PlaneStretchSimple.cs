using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlaneStretchSimple : MonoBehaviour
{
    public Transform plane;          // Plane to scale
    public Transform anchorPoint;    // Fixed side
    public Transform handlePoint;    // This object (grab handle)

    public float minLength = 0.5f;
    public float maxLength = 5f;

    private XRGrabInteractable grab;
    private bool isGrabbed = false;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
        grab.selectExited.AddListener(OnRelease);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        isGrabbed = true;
    }

    void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
    }

    void Update()
    {
        if (!isGrabbed) return;

        // Distance between anchor and handle
        float distance = Vector3.Distance(anchorPoint.position, handlePoint.position);

        // Clamp length
        float clampedLength = Mathf.Clamp(distance, minLength, maxLength);

        // Apply scale (Z axis stretch example)
        Vector3 newScale = plane.localScale;
        newScale.z = clampedLength;
        plane.localScale = newScale;

        // Move plane so one side stays fixed at anchor
        Vector3 midPoint = (anchorPoint.position + handlePoint.position) / 2f;
        plane.position = midPoint;

        // Optional: align plane direction
        plane.forward = (handlePoint.position - anchorPoint.position).normalized;
    }
}
