using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    public static HighlightManager Instance;

    Dictionary<string, HighlightObject> objects =
        new Dictionary<string, HighlightObject>();

    void Awake()
    {
        Instance = this;
    }

    public void Register(string id, HighlightObject obj)
    {
        objects[id] = obj;
    }

    public void Highlight(string id)
    {
        if (objects.ContainsKey(id))
            objects[id].EnableHighlight();
    }

    public void Disable(string id)
    {
        if (objects.ContainsKey(id))
            objects[id].DisableHighlight();
    }
}