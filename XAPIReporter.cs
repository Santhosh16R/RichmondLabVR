using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections;

public class XAPIReporter : MonoBehaviour
{
    string endpoint =
        "https://cloud.scorm.com/lrs/XXXX/xAPI/statements";

    string key = "YOUR_KEY";
    string secret = "YOUR_SECRET";

    public void SendCompletion(int score)
    {
        StartCoroutine(SendXAPI(score));
    }

    IEnumerator SendXAPI(int score)
    {
        string auth =
            System.Convert.ToBase64String(
            Encoding.ASCII.GetBytes(key + ":" + secret));

        string json =
        @"{
            ""actor"":{""name"":""QuestUser""},
            ""verb"":{""id"":""http://adlnet.gov/expapi/verbs/completed""},
            ""object"":{""id"":""RichmondLabVR""},
            ""result"":{""score"":{""raw"":"+score+@"}}
        }";

        UnityWebRequest req =
            new UnityWebRequest(endpoint,"POST");

        byte[] body =
            Encoding.UTF8.GetBytes(json);

        req.uploadHandler =
            new UploadHandlerRaw(body);

        req.downloadHandler =
            new DownloadHandlerBuffer();

        req.SetRequestHeader(
            "Content-Type",
            "application/json");

        req.SetRequestHeader(
            "Authorization",
            "Basic " + auth);

        yield return req.SendWebRequest();

        Debug.Log(req.result);
    }
}
