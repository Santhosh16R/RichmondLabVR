using UnityEngine;
using UnityEngine.SceneManagement;

public class DeepLinkManager : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Application.deepLinkActivated += OnDeepLink;

        if (!string.IsNullOrEmpty(Application.absoluteURL))
            OnDeepLink(Application.absoluteURL);
    }

    void OnDeepLink(string url)
    {
        Debug.Log("DeepLink Received: " + url);

        if (url.StartsWith("richmondlab://launch"))
        {
            SceneManager.LoadScene(1); // Training Scene
        }
    }
}
