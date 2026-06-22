using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class RestTokenExchangeResponse
{
    public string accessToken;
    public string access_token;
    public string token;
    public string idToken;
}

[Serializable]
public class RestQuestSessionRef
{
    public string tokenId;
    public string bridgePlayerId;
    public string playerId;
}

[Serializable]
public class RestCloudCodeSessionResult
{
    public bool success;
    public string tokenId;
    public string bridgePlayerId;
    public string playerId;
    public string questPlayerId;
    public string status;
    public float score;
    public bool passed;
    public string customData;
}

[Serializable]
public class RestCloudCodeResponse
{
    public RestCloudCodeSessionResult output;
}

public class QuestScormRestBridge : MonoBehaviour
{
    [Header("Unity Project")]
    [SerializeField] private string projectId = "99f57770-6792-4c65-a2c8-1ad5393c2b04";
    [SerializeField] private string environmentId = "af2f7a7d-a0f1-41ae-95a7-6d19fd4429a0";
    [SerializeField] private string environmentName = "production";

    [Header("Prototype Credential")]
    [Tooltip("Base64 part from: Authorization: Basic <base64>. Do not ship this in production.")]
    [SerializeField] private string serviceBasicAuth = "YWYxN2Q5NmItYzNiNi00MWY5LWE4MTYtNWVjMGVhNTkzNzg4OlRMdWd5RlBJTml0R29qc2VQd1pNMGRGVVhIX1BXSG1S";

    [Header("Cloud Code Script Names")]
    [SerializeField] private string createSessionScriptName = "createsession";
    [SerializeField] private string startSessionScriptName = "StartSession";
    [SerializeField] private string sessionStatusScriptName = "sessionstatus";
    [SerializeField] private string completeSessionScriptName = "CompleteSession";

    [Header("Test Inputs")]
    [SerializeField] private string testPlayerId = "SADddd";
    [SerializeField] private string testBridgePlayerId = "SADddd";
    [SerializeField] private string testTokenId = "TEST-ABC123";
    [SerializeField] private float testScore = 100f;
    [SerializeField] private bool testPassed = true;
    [TextArea(2, 5)]
    [SerializeField] private string testCustomDataJson = "{\"source\":\"quest-rest-test\"}";

    [Header("Runtime")]
    [SerializeField] private string bearerToken;
    [SerializeField] private RestQuestSessionRef activeSession;
    [SerializeField] private string lastStatus = "idle";
    [TextArea(3, 8)]
    [SerializeField] private string lastResponse;

    public RestQuestSessionRef ActiveSession => activeSession;
    public string LastStatus => lastStatus;
    public string LastResponse => lastResponse;

    [ContextMenu("REST TEST / 1 Exchange Bearer Token")]
    public void TestExchangeBearerToken()
    {
        StartCoroutine(ExchangeBearerTokenCoroutine(null));
    }

    [ContextMenu("REST TEST / Clear Bearer Token")]
    public void TestClearBearerToken()
    {
        bearerToken = "";
        SetStatus("bearer-token-cleared");
    }

    [ContextMenu("REST TEST / 2 Create Session")]
    public void TestCreateSession()
    {
        StartCoroutine(CreateSessionCoroutine(testPlayerId, null));
    }

    [ContextMenu("REST TEST / 3 Start Session")]
    public void TestStartSession()
    {
        StartCoroutine(StartSessionCoroutine(testTokenId, testBridgePlayerId, FirstNonEmpty(testPlayerId, testBridgePlayerId), null));
    }

    [ContextMenu("REST TEST / 3A Create Then Start Session")]
    public void TestCreateThenStartSession()
    {
        StartCoroutine(CreateThenStartSessionCoroutine(testPlayerId, null));
    }

    [ContextMenu("REST TEST / 4 Session Status")]
    public void TestSessionStatus()
    {
        var session = GetInspectorOrActiveSession();
        StartCoroutine(SessionStatusCoroutine(session.tokenId, session.bridgePlayerId, session.playerId, null));
    }

    [ContextMenu("REST TEST / 5 Complete Session")]
    public void TestCompleteSession()
    {
        var session = GetInspectorOrActiveSession();
        StartCoroutine(CompleteSessionCoroutine(session.tokenId, session.bridgePlayerId, session.playerId, testScore, testPassed, testCustomDataJson, null));
    }

    public void CreateSession(string playerId)
    {
        StartCoroutine(CreateSessionCoroutine(playerId, null));
    }

    public void StartSession(string tokenId, string bridgePlayerId, string playerId)
    {
        StartCoroutine(StartSessionCoroutine(tokenId, bridgePlayerId, playerId, null));
    }

    public void CreateThenStartSession(string createSessionPlayerId)
    {
        StartCoroutine(CreateThenStartSessionCoroutine(createSessionPlayerId, null));
    }

    public void SessionStatus(string tokenId, string bridgePlayerId, string playerId)
    {
        StartCoroutine(SessionStatusCoroutine(tokenId, bridgePlayerId, playerId, null));
    }

    public void CompleteSession(string tokenId, string bridgePlayerId, string playerId, float score, bool passed, string customDataJson)
    {
        StartCoroutine(CompleteSessionCoroutine(tokenId, bridgePlayerId, playerId, score, passed, customDataJson, null));
    }

    public IEnumerator CreateSessionCoroutine(string playerId, Action<RestCloudCodeSessionResult> onComplete)
    {
        yield return EnsureBearerTokenCoroutine();

        playerId = NormalizePlayerIdInput(playerId);

        var json =
            "{\"params\":{" +
            "\"playerId\":\"" + EscapeJson(playerId) + "\"," +
            "\"bridgePlayerId\":\"" + EscapeJson(playerId) + "\"" +
            "}}";
        yield return CallCloudCodeWithBearerCoroutine(createSessionScriptName, json, result =>
        {
            if (result != null)
            {
                activeSession = new RestQuestSessionRef
                {
                    tokenId = result.tokenId,
                    playerId = result.playerId,
                    bridgePlayerId = string.IsNullOrWhiteSpace(result.bridgePlayerId) ? result.playerId : result.bridgePlayerId
                };
            }

            onComplete?.Invoke(result);
        });
    }

    public IEnumerator CreateThenStartSessionCoroutine(string createSessionPlayerId, Action<RestCloudCodeSessionResult> onComplete)
    {
        createSessionPlayerId = NormalizePlayerIdInput(createSessionPlayerId);
        RestCloudCodeSessionResult createdSession = null;

        yield return CreateSessionCoroutine(createSessionPlayerId, result =>
        {
            createdSession = result;
        });

        if (createdSession == null || string.IsNullOrWhiteSpace(createdSession.tokenId))
        {
            SetStatus("create-before-start-failed");
            Debug.LogError("[QuestScormRestBridge] Cannot start session because createsession did not return tokenId. Response: " + lastResponse);
            yield break;
        }

        var createdPlayerId = FirstNonEmpty(createdSession.playerId, createSessionPlayerId);
        var createdBridgePlayerId = FirstNonEmpty(createdSession.bridgePlayerId, createdPlayerId);

        if (string.IsNullOrWhiteSpace(createdPlayerId))
        {
            SetStatus("create-before-start-missing-playerid");
            Debug.LogError("[QuestScormRestBridge] Cannot start session because createsession did not return playerId.");
            yield break;
        }

        SetStatus("created-session-playerid: " + createdPlayerId);

        yield return StartSessionCoroutine(
            createdSession.tokenId,
            createdBridgePlayerId,
            createdPlayerId,
            onComplete);
    }

    public IEnumerator StartSessionCoroutine(string tokenId, string bridgePlayerId, string playerId, Action<RestCloudCodeSessionResult> onComplete)
    {
        yield return RefreshBearerTokenCoroutine();

        var json =
            "{\"params\":{" +
            "\"tokenId\":\"" + EscapeJson(tokenId) + "\"," +
            "\"bridgePlayerId\":\"" + EscapeJson(bridgePlayerId) + "\"," +
            "\"playerId\":\"" + EscapeJson(playerId) + "\"" +
            "}}";

        yield return CallCloudCodeWithBearerCoroutine(startSessionScriptName, json, result =>
        {
            if (result != null)
            {
                activeSession = new RestQuestSessionRef
                {
                    tokenId = tokenId,
                    bridgePlayerId = bridgePlayerId,
                    playerId = playerId
                };
            }

            onComplete?.Invoke(result);
        });
    }

    public IEnumerator SessionStatusCoroutine(string tokenId, string bridgePlayerId, string playerId, Action<RestCloudCodeSessionResult> onComplete)
    {
        yield return EnsureBearerTokenCoroutine();

        var json =
            "{\"params\":{" +
            "\"tokenId\":\"" + EscapeJson(tokenId) + "\"," +
            "\"bridgePlayerId\":\"" + EscapeJson(bridgePlayerId) + "\"," +
            "\"playerId\":\"" + EscapeJson(playerId) + "\"" +
            "}}";

        yield return CallCloudCodeWithBearerCoroutine(sessionStatusScriptName, json, onComplete);
    }

    public IEnumerator CompleteSessionCoroutine(
        string tokenId,
        string bridgePlayerId,
        string playerId,
        float score,
        bool passed,
        string customDataJson,
        Action<RestCloudCodeSessionResult> onComplete)
    {
        yield return EnsureBearerTokenCoroutine();

        var json =
            "{\"params\":{" +
            "\"tokenId\":\"" + EscapeJson(tokenId) + "\"," +
            "\"bridgePlayerId\":\"" + EscapeJson(bridgePlayerId) + "\"," +
            "\"playerId\":\"" + EscapeJson(playerId) + "\"," +
            "\"score\":" + score.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," +
            "\"passed\":" + (passed ? "true" : "false") + "," +
            "\"customData\":\"" + EscapeJson(customDataJson) + "\"" +
            "}}";

        yield return CallCloudCodeWithBearerCoroutine(completeSessionScriptName, json, onComplete);
    }

    private IEnumerator EnsureBearerTokenCoroutine()
    {
        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            yield break;
        }

        yield return ExchangeBearerTokenCoroutine(null);
    }

    private IEnumerator RefreshBearerTokenCoroutine()
    {
        SetStatus("refreshing-bearer-token");
        bearerToken = "";
        yield return ExchangeBearerTokenCoroutine(null);
    }

    private IEnumerator ExchangeBearerTokenCoroutine(Action<string> onComplete)
    {
        SetStatus("exchanging-bearer-token");

        var url = "https://services.api.unity.com/auth/v1/token-exchange?projectId=" +
                  UnityWebRequest.EscapeURL(projectId) +
                  "&environmentId=" +
                  UnityWebRequest.EscapeURL(environmentId);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            var body = System.Text.Encoding.UTF8.GetBytes("{}");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Basic " + serviceBasicAuth);

            yield return request.SendWebRequest();

            lastResponse = request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success)
            {
                SetStatus("token-exchange-failed");
                Debug.LogError("[QuestScormRestBridge] Token exchange failed: " + request.responseCode + " " + lastResponse);
                yield break;
            }

            var response = JsonUtility.FromJson<RestTokenExchangeResponse>(lastResponse);
            bearerToken = FirstNonEmpty(response.accessToken, response.access_token, response.token, response.idToken);

            if (string.IsNullOrWhiteSpace(bearerToken))
            {
                SetStatus("token-exchange-missing-access-token");
                Debug.LogError("[QuestScormRestBridge] Token exchange response did not include an access token: " + lastResponse);
                yield break;
            }

            SetStatus("bearer-token-ready");
            onComplete?.Invoke(bearerToken);
        }
    }

    private IEnumerator CallCloudCodeWithBearerCoroutine(string scriptName, string jsonBody, Action<RestCloudCodeSessionResult> onComplete)
    {
        yield return EnsureBearerTokenCoroutine();
        yield return CallCloudCodeCoroutine(scriptName, jsonBody, onComplete);

        if (lastStatus == scriptName + "-unauthorized")
        {
            bearerToken = "";
            yield return ExchangeBearerTokenCoroutine(null);
            yield return CallCloudCodeCoroutine(scriptName, jsonBody, onComplete);
        }
    }

    private IEnumerator CallCloudCodeCoroutine(string scriptName, string jsonBody, Action<RestCloudCodeSessionResult> onComplete)
    {
        SetStatus("calling-" + scriptName);

        var url = "https://cloud-code.services.api.unity.com/v1/projects/" +
                  UnityWebRequest.EscapeURL(projectId) +
                  "/scripts/" +
                  UnityWebRequest.EscapeURL(scriptName);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            var body = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            request.SetRequestHeader("UnityEnvironment", environmentName);

            yield return request.SendWebRequest();

            lastResponse = request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 401)
                {
                    SetStatus(scriptName + "-unauthorized");
                    Debug.LogError("[QuestScormRestBridge] " + scriptName + " unauthorized. Bearer token may be expired: " + lastResponse);
                    yield break;
                }

                SetStatus(scriptName + "-failed");
                Debug.LogError("[QuestScormRestBridge] " + scriptName + " failed: " + request.responseCode + " " + lastResponse);
                yield break;
            }

            var result = ParseCloudCodeResult(lastResponse);
            SetStatus(string.IsNullOrWhiteSpace(result.status) ? scriptName + "-ok" : result.status);
            Debug.Log("[QuestScormRestBridge] " + scriptName + " response: " + lastResponse);
            onComplete?.Invoke(result);
        }
    }

    private RestCloudCodeSessionResult ParseCloudCodeResult(string json)
    {
        var wrapped = JsonUtility.FromJson<RestCloudCodeResponse>(json);
        if (wrapped != null && wrapped.output != null)
        {
            return wrapped.output;
        }

        return JsonUtility.FromJson<RestCloudCodeSessionResult>(json);
    }

    private RestQuestSessionRef GetInspectorOrActiveSession()
    {
        if (activeSession != null && !string.IsNullOrWhiteSpace(activeSession.tokenId))
        {
            return activeSession;
        }

        return new RestQuestSessionRef
        {
            tokenId = testTokenId,
            bridgePlayerId = testBridgePlayerId,
            playerId = FirstNonEmpty(testPlayerId, testBridgePlayerId)
        };
    }

    private void SetStatus(string status)
    {
        lastStatus = status;
        Debug.Log("[QuestScormRestBridge] " + status);
    }

    private static string FirstNonEmpty(params string[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return "";
    }

    private static string EscapeJson(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    private static string NormalizePlayerIdInput(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("{") && trimmed.EndsWith("}"))
        {
            try
            {
                var sessionRef = JsonUtility.FromJson<RestQuestSessionRef>(trimmed);
                return FirstNonEmpty(sessionRef.playerId, sessionRef.bridgePlayerId, trimmed);
            }
            catch
            {
                return trimmed;
            }
        }

        return trimmed;
    }
}
