using UnityEngine;
using UnityEngine.SceneManagement;

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
        Debug.Log("DeepLink: " + url);

        string token = GetToken(url);

        Authenticate(token);
    }

    string GetToken(string url)
    {
        System.Uri uri = new System.Uri(url);
        return uri.Query.Replace("?token=","");
    }

    void Authenticate(string token)
    {
        Debug.Log("Authenticated: " + token);

        SceneManager.LoadScene("TrainingScene");
    }
}
