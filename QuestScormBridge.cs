using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudCode;
using Unity.Services.Core;
using UnityEngine;

[Serializable]
public class QuestScormSessionRef
{
    public string tokenId;
    public string bridgePlayerId;
}

[Serializable]
public class QuestScormStatus
{
    public bool success;
    public string tokenId;
    public string bridgePlayerId;
    public string questPlayerId;
    public string status;
    public float score;
    public bool passed;
    public string customData;
}

public class QuestScormBridge : MonoBehaviour
{
    [Header("Cloud Code Script Names")]
    [SerializeField] private string startSessionScriptName = "StartSession";
    [SerializeField] private string sessionStatusScriptName = "sessionstatus";
    [SerializeField] private string completeSessionScriptName = "CompleteSession";

    [Header("Inspector Test Inputs")]
    [TextArea(3, 6)]
    [SerializeField] private string testQrJson = "{\"tokenId\":\"TEST-ABC123\",\"bridgePlayerId\":\"scorm-local-learner\"}";
    [SerializeField] private string testTokenId = "TEST-ABC123";
    [SerializeField] private string testBridgePlayerId = "scorm-local-learner";
    [SerializeField] private float testScore = 100f;
    [SerializeField] private bool testPassed = true;
    [TextArea(2, 5)]
    [SerializeField] private string testCustomDataJson = "{\"source\":\"quest-test\"}";

    [Header("Runtime State")]
    [SerializeField] private QuestScormSessionRef activeSession;
    [SerializeField] private string lastStatus = "idle";

    public QuestScormSessionRef ActiveSession => activeSession;
    public string LastStatus => lastStatus;

    public event Action<string> StatusChanged;
    public event Action<QuestScormSessionRef> SessionStarted;
    public event Action<QuestScormStatus> SessionCompleted;

    [ContextMenu("TEST / 1 Initialize Unity Services")]
    public async void TestInitialize()
    {
        await RunTestAction("Initialize", InitializeAsync);
    }

    [ContextMenu("TEST / 2 Start Session From QR JSON")]
    public async void TestStartSessionFromQrJson()
    {
        await RunTestAction("Start from QR JSON", () => StartSessionFromQrJsonAsync(testQrJson));
    }

    [ContextMenu("TEST / 2 Start Session From Fields")]
    public async void TestStartSessionFromFields()
    {
        await RunTestAction("Start from fields", () => StartSessionAsync(testTokenId, testBridgePlayerId));
    }

    [ContextMenu("TEST / 3 Check Session Status")]
    public async void TestCheckSessionStatus()
    {
        await RunTestAction("Check status", async () =>
        {
            var result = await GetSessionStatusAsync();
            Debug.Log("[QuestScormBridge] Status result: " + JsonUtility.ToJson(result));
        });
    }

    [ContextMenu("TEST / 4 Complete Session")]
    public async void TestCompleteSession()
    {
        await RunTestAction("Complete session", async () =>
        {
            var result = await CompleteSessionAsync(testScore, testPassed, testCustomDataJson);
            Debug.Log("[QuestScormBridge] Complete result: " + JsonUtility.ToJson(result));
        });
    }

    [ContextMenu("TEST / Clear Active Session")]
    public void TestClearSession()
    {
        ClearSession();
    }

    public async Task InitializeAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized &&
            AuthenticationService.Instance.IsSignedIn)
        {
            SetStatus("signed-in: " + AuthenticationService.Instance.PlayerId);
            return;
        }

        SetStatus("initializing-unity-services");
        await UnityServices.InitializeAsync();
        SetStatus("unity-services-ready");

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            SetStatus("signing-in-anonymously");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        SetStatus("signed-in: " + AuthenticationService.Instance.PlayerId);
    }

    public async Task StartSessionFromQrJsonAsync(string qrJson)
    {
        if (string.IsNullOrWhiteSpace(qrJson))
        {
            throw new Exception("QR JSON input is empty.");
        }

        var sessionRef = JsonUtility.FromJson<QuestScormSessionRef>(qrJson);
        await StartSessionAsync(sessionRef);
    }

    public async Task StartSessionAsync(string tokenId, string bridgePlayerId)
    {
        await StartSessionAsync(new QuestScormSessionRef
        {
            tokenId = tokenId,
            bridgePlayerId = bridgePlayerId
        });
    }

    public async Task StartSessionAsync(QuestScormSessionRef sessionRef)
    {
        await InitializeAsync();
        ValidateSessionRef(sessionRef);

        SetStatus("starting-session");

        var args = new Dictionary<string, object>
        {
            { "tokenId", sessionRef.tokenId.Trim().ToUpperInvariant() },
            { "bridgePlayerId", sessionRef.bridgePlayerId.Trim() }
        };

        await CloudCodeService.Instance.CallEndpointAsync<Dictionary<string, object>>(startSessionScriptName, args);

        activeSession = sessionRef;
        activeSession.tokenId = activeSession.tokenId.Trim().ToUpperInvariant();
        activeSession.bridgePlayerId = activeSession.bridgePlayerId.Trim();

        SetStatus("started");
        SessionStarted?.Invoke(activeSession);
    }

    public async Task<QuestScormStatus> GetSessionStatusAsync()
    {
        EnsureActiveSession();
        await InitializeAsync();

        var args = MakeSessionArgs();
        var result = await CloudCodeService.Instance.CallEndpointAsync<QuestScormStatus>(sessionStatusScriptName, args);

        if (!string.IsNullOrWhiteSpace(result.status))
        {
            SetStatus(result.status);
        }

        return result;
    }

    public async Task<QuestScormStatus> CompleteSessionAsync(float score, bool passed, string customDataJson = null)
    {
        EnsureActiveSession();
        await InitializeAsync();

        SetStatus("completing-session");

        var args = MakeSessionArgs();
        args["score"] = score;
        args["passed"] = passed;

        if (!string.IsNullOrWhiteSpace(customDataJson))
        {
            args["customData"] = customDataJson;
        }

        var result = await CloudCodeService.Instance.CallEndpointAsync<QuestScormStatus>(completeSessionScriptName, args);

        SetStatus("completed");
        SessionCompleted?.Invoke(result);
        return result;
    }

    public void ClearSession()
    {
        activeSession = null;
        SetStatus("idle");
    }

    private async Task RunTestAction(string label, Func<Task> action)
    {
        try
        {
            SetStatus("test: " + label);
            await action();
        }
        catch (Exception ex)
        {
            SetStatus("test failed: " + ex.Message);
            Debug.LogException(ex);
        }
    }

    private Dictionary<string, object> MakeSessionArgs()
    {
        return new Dictionary<string, object>
        {
            { "tokenId", activeSession.tokenId },
            { "bridgePlayerId", activeSession.bridgePlayerId }
        };
    }

    private void ValidateSessionRef(QuestScormSessionRef sessionRef)
    {
        if (sessionRef == null)
        {
            throw new Exception("Session reference is empty.");
        }

        if (string.IsNullOrWhiteSpace(sessionRef.tokenId))
        {
            throw new Exception("Session tokenId is required.");
        }

        if (string.IsNullOrWhiteSpace(sessionRef.bridgePlayerId))
        {
            throw new Exception("Session bridgePlayerId is required.");
        }
    }

    private void EnsureActiveSession()
    {
        ValidateSessionRef(activeSession);
    }

    private void SetStatus(string status)
    {
        lastStatus = status;
        Debug.Log("[QuestScormBridge] " + status);
        StatusChanged?.Invoke(status);
    }
}
