using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabNotifier : MonoBehaviour
{
    public string objectID;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);

        HighlightObject h = GetComponent<HighlightObject>();

        if (h != null)
            HighlightManager.Instance.Register(objectID, h);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        EventBus.OnObjectGrabbed?.Invoke(objectID);
    }
}