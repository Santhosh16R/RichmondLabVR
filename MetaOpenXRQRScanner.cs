using UnityEngine;
using ZXing;
using System.Collections;

public class MetaQuestQRScanner : MonoBehaviour
{
    WebCamTexture camTexture;
    BarcodeReader reader;
    bool scanning = false;

    IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(
            UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Camera permission denied");
            yield break;
        }

        reader = new BarcodeReader
        {
            AutoRotate = true,
            TryInverted = true
        };

        WebCamDevice[] devices = WebCamTexture.devices;

        foreach (var d in devices)
            Debug.Log("Camera Found: " + d.name);

        // Quest RGB camera
        camTexture = new WebCamTexture(devices[0].name, 1280, 720, 30);
        camTexture.Play();

        scanning = true;

        InvokeRepeating(nameof(ScanQR), 2f, 0.3f);
    }

    void ScanQR()
    {
        if (!scanning || camTexture.width < 100)
            return;

        var result = reader.Decode(
            camTexture.GetPixels32(),
            camTexture.width,
            camTexture.height);

        if (result != null)
        {
            scanning = false;

            Debug.Log("✅ QR DETECTED: " + result.Text);
            OnQRDetected(result.Text);
        }
    }

    void OnQRDetected(string data)
    {
        Debug.Log("QR DATA: " + data);
    }
}
