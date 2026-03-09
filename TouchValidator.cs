using UnityEngine;

public class TouchValidator : MonoBehaviour
{
    public string objectA;
    public string objectB;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(objectB))
        {
            EventBus.OnObjectsTouched?.Invoke(objectA, objectB);
        }
    }
}