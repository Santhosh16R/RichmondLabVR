using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    Renderer rend;
    Material originalMat;

    public Material highlightMaterial;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        originalMat = rend.material;
    }

    public void EnableHighlight()
    {
        rend.material = highlightMaterial;
    }

    public void DisableHighlight()
    {
        rend.material = originalMat;
    }
}