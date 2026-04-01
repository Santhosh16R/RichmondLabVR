using UnityEngine;
using System;

public class ScormBridgeSender : MonoBehaviour
{
    public string bridgeURL =
        "https://yourdomain/index.html";

    float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    // --------------------------
    // SEND PROGRESS
    // --------------------------
    public void SendProgress(string step)
    {
        string url =
            bridgeURL +
            "?step=" + step;

        Application.OpenURL(url);
    }

    // --------------------------
    // SEND SCORE
    // --------------------------
    public void SendScore(int score)
    {
        string url =
            bridgeURL +
            "?score=" + score;

        Application.OpenURL(url);
    }

    // --------------------------
    // COMPLETE COURSE
    // --------------------------
    public void CompleteTraining(int score)
    {
        string url =
            bridgeURL +
            "?score=" + score +
            "&complete=true";

        Application.OpenURL(url);
    }
}
