using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class VRScormController : MonoBehaviour
{
    [Header("Server Configuration")]
    [SerializeField] private string serverBaseUrl = "https://scormservermy.onrender.com";

    private string activeToken = "";
    private bool isSessionActive = false;

    // Data structure for custom analytics
    [System.Serializable]
    public class VRAnalytics
    {
        public string questionsCorrect; // e.g., "10/13"
        public string avgTime;          // e.g., "6.5 s"
        public string maxTime;          // e.g., "7.8 s"
    }

    /// <summary>
    /// Call this immediately when the Quest 3 scans the QR code.
    /// </summary>
    public void ValidateToken(string scannedToken)
    {
        activeToken = scannedToken;
        isSessionActive = true;
        Debug.Log($"[SCORM] Token Validated: {activeToken}. Starting session...");

        // Notify bridge: "Training in Progress"
        StartCoroutine(SendUpdate(activeToken, "progress", 0, false, null));
    }

    /// <summary>
    /// Call this when the training is finished.
    /// </summary>
    public void CompleteTraining(int finalScore, bool didPass, string correctRatio, string avgTime, string longestTime)
    {
        if (!isSessionActive)
        {
            Debug.LogError("[SCORM] No active session to complete!");
            return;
        }

        VRAnalytics stats = new VRAnalytics
        {
            questionsCorrect = correctRatio,
            avgTime = avgTime,
            maxTime = longestTime
        };

        string jsonAnalytics = JsonUtility.ToJson(stats);
        Debug.Log("[SCORM] Completing session with analytics: " + jsonAnalytics);

        StartCoroutine(SendUpdate(activeToken, "completed", finalScore, didPass, jsonAnalytics));

        isSessionActive = false; // Reset for next session
    }

    private IEnumerator SendUpdate(string token, string status, int score, bool passed, string customData)
    {
        string url = $"{serverBaseUrl}/complete-session";

        // Construct JSON Payload manually to ensure compatibility with Node.js req.body
        // Note: we use .ToLower() for the boolean to match JavaScript expectations
        string jsonPayload = $"{{\"tokenId\":\"{token}\", \"status\":\"{status}\", \"score\":{score}, \"passed\":{passed.ToString().ToLower()}, \"customData\":{(customData != null ? "\"" + customData.Replace("\"", "\\\"") + "\"" : "null")}}}";

        byte[] jsonToSend = Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SCORM] Sync Failed: {request.error} | {request.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"[SCORM] Sync Successful: {status}");
            }
        }
    }

    [ContextMenu("Simulate Completion")]
    public void TestComplete()
    {
        Debug.Log("--- Testing Completion ---");
        CompleteTraining(
            50,
            true,
            "10/13",
            "6.8 s",
            "7.5 s"
        );
    }
}