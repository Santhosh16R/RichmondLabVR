using UnityEngine;

public class ScormAnalytics : MonoBehaviour
{
    public void SendProgress(int step)
    {
        Application.OpenURL(
        "javascript:window.postMessage({type:'progress',step:'"
        + step + "'},'*');");
    }

    public void SendScore(int score)
    {
        Application.OpenURL(
        "javascript:window.postMessage({type:'score',score:"
        + score + "},'*');");
    }

    public void CompleteCourse()
    {
        Application.OpenURL(
        "javascript:window.postMessage({type:'complete'},'*');");
    }
}
