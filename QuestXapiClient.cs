using System;
using System.Collections;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class XapiLaunchPayload
{
    public int version;
    public string launchType;
    public string registration;
    public string learnerId;
    public string learnerName;
    public string learnerEmail;
    public string actorAccountHomePage;
    public string activityId;
    public string activityName;
    public string lrsEndpoint;
    public string platform;
    public string completionCode;
}

public class QuestXapiClient : MonoBehaviour
{
    [Header("SCORM Cloud Activity Provider")]
    [SerializeField] private string activityProviderKey;
    [SerializeField] private string activityProviderSecret;

    [Header("Testing")]
    [SerializeField, TextArea(3, 8)] private string testQrText;

    private XapiLaunchPayload launch;

    public string CompletionCode => launch != null ? launch.completionCode : "";

    public void OnQrScanned(string scannedText)
    {
        try
        {
            launch = ParseLaunchPayload(scannedText);
            Debug.Log("xAPI launch parsed for learner: " + launch.learnerName);
            StartCoroutine(SendInitialized());
        }
        catch (Exception ex)
        {
            Debug.LogError("QR parse failed: " + ex.Message);
        }
    }

    public IEnumerator SendInitialized()
    {
        string statement = BuildCourseStatement(
            "http://adlnet.gov/expapi/verbs/initialized",
            "initialized",
            null
        );

        yield return SendStatement(statement);
    }

    public IEnumerator SendProgress(string stepName, float progressPercent)
    {
        string resultJson =
            "\"result\":{" +
                "\"extensions\":{" +
                    "\"https://richmondlab.com/xapi/extensions/step-name\":\"" + Escape(stepName) + "\"," +
                    "\"https://richmondlab.com/xapi/extensions/progress-percent\":" + Number(progressPercent) +
                "}" +
            "}";

        string statement = BuildCourseStatement(
            "http://adlnet.gov/expapi/verbs/progressed",
            "progressed",
            resultJson
        );

        yield return SendStatement(statement);
    }

    public IEnumerator SendCompleted(float scoreRaw, bool passed = true)
    {
        float scaled = Mathf.Clamp01(scoreRaw / 100f);

        string resultJson =
            "\"result\":{" +
                "\"score\":{" +
                    "\"scaled\":" + Number(scaled) + "," +
                    "\"raw\":" + Number(scoreRaw) + "," +
                    "\"min\":0," +
                    "\"max\":100" +
                "}," +
                "\"success\":" + Bool(passed) + "," +
                "\"completion\":true" +
            "}";

        string statement = BuildCourseStatement(
            "http://adlnet.gov/expapi/verbs/completed",
            "completed",
            resultJson
        );

        yield return SendStatement(statement);

        Debug.Log("Quest completion code for SCORM bridge: " + CompletionCode);
    }

    public IEnumerator SendTerminated()
    {
        string statement = BuildCourseStatement(
            "http://adlnet.gov/expapi/verbs/terminated",
            "terminated",
            null
        );

        yield return SendStatement(statement);
    }

    private XapiLaunchPayload ParseLaunchPayload(string scannedText)
    {
        const string prefix = "xapi-launch:";

        if (string.IsNullOrWhiteSpace(scannedText) || !scannedText.StartsWith(prefix))
        {
            throw new Exception("Invalid xAPI launch QR.");
        }

        string encoded = scannedText.Substring(prefix.Length);
        string json = Encoding.UTF8.GetString(Base64UrlDecode(encoded));

        XapiLaunchPayload payload = JsonUtility.FromJson<XapiLaunchPayload>(json);

        if (payload == null || string.IsNullOrEmpty(payload.lrsEndpoint))
        {
            throw new Exception("Launch payload missing LRS endpoint.");
        }

        if (string.IsNullOrEmpty(payload.registration))
        {
            throw new Exception("Launch payload missing registration.");
        }

        return payload;
    }

    private string BuildCourseStatement(string verbId, string verbDisplay, string optionalResultJson)
    {
        EnsureLaunchReady();

        string actorJson = BuildActorJson();

        string objectJson =
            "\"object\":{" +
                "\"objectType\":\"Activity\"," +
                "\"id\":\"" + Escape(launch.activityId) + "\"," +
                "\"definition\":{" +
                    "\"name\":{\"en-US\":\"" + Escape(launch.activityName) + "\"}," +
                    "\"type\":\"http://adlnet.gov/expapi/activities/course\"" +
                "}" +
            "}";

        string contextJson =
            "\"context\":{" +
                "\"registration\":\"" + Escape(launch.registration) + "\"," +
                "\"platform\":\"" + Escape(string.IsNullOrEmpty(launch.platform) ? "Meta Quest 3" : launch.platform) + "\"" +
            "}";

        return "{" +
            actorJson + "," +
            "\"verb\":{\"id\":\"" + Escape(verbId) + "\",\"display\":{\"en-US\":\"" + Escape(verbDisplay) + "\"}}," +
            objectJson + "," +
            (string.IsNullOrEmpty(optionalResultJson) ? "" : optionalResultJson + ",") +
            contextJson + "," +
            "\"timestamp\":\"" + DateTime.UtcNow.ToString("o") + "\"" +
        "}";
    }

    private string BuildActorJson()
    {
        string name = string.IsNullOrEmpty(launch.learnerName) ? "Unknown Learner" : launch.learnerName;

        if (!string.IsNullOrEmpty(launch.learnerEmail))
        {
            return "\"actor\":{" +
                "\"objectType\":\"Agent\"," +
                "\"name\":\"" + Escape(name) + "\"," +
                "\"mbox\":\"mailto:" + Escape(launch.learnerEmail) + "\"" +
            "}";
        }

        string homePage = string.IsNullOrEmpty(launch.actorAccountHomePage)
            ? "https://cloud.scorm.com"
            : launch.actorAccountHomePage;

        string learnerId = string.IsNullOrEmpty(launch.learnerId)
            ? "unknown-learner"
            : launch.learnerId;

        return "\"actor\":{" +
            "\"objectType\":\"Agent\"," +
            "\"name\":\"" + Escape(name) + "\"," +
            "\"account\":{" +
                "\"homePage\":\"" + Escape(homePage) + "\"," +
                "\"name\":\"" + Escape(learnerId) + "\"" +
            "}" +
        "}";
    }

    private IEnumerator SendStatement(string statementJson)
    {
        EnsureLaunchReady();

        byte[] body = Encoding.UTF8.GetBytes(statementJson);

        using (UnityWebRequest request = new UnityWebRequest(launch.lrsEndpoint, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            string rawAuth = activityProviderKey + ":" + activityProviderSecret;
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawAuth));

            request.SetRequestHeader("Authorization", "Basic " + auth);
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("X-Experience-API-Version", "1.0.3");

            Debug.Log("Sending xAPI statement: " + statementJson);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("xAPI failed: " + request.responseCode + " " + request.downloadHandler.text);
            }
            else
            {
                Debug.Log("xAPI sent successfully: " + request.downloadHandler.text);
            }
        }
    }

    private void EnsureLaunchReady()
    {
        if (launch == null)
        {
            throw new Exception("No xAPI launch loaded. Scan QR first.");
        }

        if (string.IsNullOrEmpty(activityProviderKey) || string.IsNullOrEmpty(activityProviderSecret))
        {
            throw new Exception("Missing SCORM Cloud Activity Provider key/secret.");
        }
    }

    private static byte[] Base64UrlDecode(string value)
    {
        string base64 = value.Replace("-", "+").Replace("_", "/");

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }

    private static string Base64UrlEncode(string value)
    {
        string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));

        return base64
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static string Escape(string value)
    {
        if (value == null)
        {
            return "";
        }

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r");
    }

    private static string Number(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static string Bool(bool value)
    {
        return value ? "true" : "false";
    }

    [ContextMenu("TEST/Create Sample QR Payload")]
    public void TestCreateSampleQrPayload()
    {
        string json =
            "{" +
                "\"version\":1," +
                "\"launchType\":\"xapi-quest\"," +
                "\"registration\":\"" + Guid.NewGuid() + "\"," +
                "\"learnerId\":\"test-learner-001\"," +
                "\"learnerName\":\"Test Learner\"," +
                "\"learnerEmail\":\"\"," +
                "\"actorAccountHomePage\":\"https://cloud.scorm.com\"," +
                "\"activityId\":\"https://richmondlab.com/xapi/activities/robotic-arm-training\"," +
                "\"activityName\":\"Robotic Arm Training\"," +
                "\"lrsEndpoint\":\"https://cloud.scorm.com/lrs/YOUR_LRS_ID/statements\"," +
                "\"platform\":\"Meta Quest 3\"," +
                "\"completionCode\":\"TEST123\"" +
            "}";

        testQrText = "xapi-launch:" + Base64UrlEncode(json);
        Debug.Log("Sample QR text created. Replace YOUR_LRS_ID before sending.");
        Debug.Log(testQrText);
    }

    [ContextMenu("TEST/Parse Test QR")]
    public void TestParseQrOnly()
    {
        launch = ParseLaunchPayload(testQrText);
        Debug.Log("Parsed test QR:");
        Debug.Log("Learner: " + launch.learnerName);
        Debug.Log("Registration: " + launch.registration);
        Debug.Log("Completion Code: " + launch.completionCode);
    }

    [ContextMenu("TEST/Send Initialized")]
    public void TestSendInitialized()
    {
        launch = ParseLaunchPayload(testQrText);
        StartCoroutine(SendInitialized());
    }

    [ContextMenu("TEST/Send Full Demo Flow")]
    public void TestSendFullDemoFlow()
    {
        launch = ParseLaunchPayload(testQrText);
        StartCoroutine(TestDemoFlow());
    }

    private IEnumerator TestDemoFlow()
    {
        yield return SendInitialized();
        yield return SendProgress("Picked up component", 25f);
        yield return SendProgress("Moved robotic arm", 60f);
        yield return SendProgress("Placed component", 90f);
        yield return SendCompleted(95f, true);
    }
}