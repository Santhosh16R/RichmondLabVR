using UnityEngine;

public class DeepLinkManager : MonoBehaviour
{
    void Awake()
    {
        Application.deepLinkActivated += OnDeepLink;

        if (!string.IsNullOrEmpty(Application.absoluteURL))
            OnDeepLink(Application.absoluteURL);
    }

    void OnDeepLink(string url)
    {
        Debug.Log("Deep Link Received: " + url);

        string token = GetToken(url);

        LaunchCourse(token);
    }

    string GetToken(string url)
    {
        var uri = new System.Uri(url);
        var query = uri.Query;

        return query.Replace("?token=", "");
    }

    void LaunchCourse(string token)
    {
        Debug.Log("Launching course with token: " + token);

        // Start VR training scene here
    }
}
