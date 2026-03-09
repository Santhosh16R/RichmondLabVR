using UnityEngine;

public class PlaceNotifier : MonoBehaviour
{
    public string objectID;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LabObject"))
        {
            EventBus.OnObjectPlaced?.Invoke(objectID);
        }
    }
}