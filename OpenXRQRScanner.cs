using UnityEngine;
using ZXing;
using System.Collections;

public class OpenXRQRScanner : MonoBehaviour
{
    WebCamTexture webcam;
    IBarcodeReader reader;
    bool scanning;

    IEnumerator Start()
    {
        // Request camera permission
        yield return Application.RequestUserAuthorization(
            UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Camera permission denied");
            yield break;
        }

        Debug.Log("Starting Quest camera...");

        foreach (var cam in WebCamTexture.devices)
            Debug.Log("Camera: " + cam.name);

        // Quest passthrough camera feed
        webcam = new WebCamTexture();
        webcam.requestedWidth = 1280;
        webcam.requestedHeight = 720;
        webcam.Play();

        reader = new BarcodeReader
        {
            AutoRotate = true,
            TryInverted = true
        };

        scanning = true;
    }

    void Update()
    {
        if (!scanning || webcam == null)
            return;

        if (webcam.width < 100)
            return;

        var result = reader.Decode(
            webcam.GetPixels32(),
            webcam.width,
            webcam.height
        );

        if (result != null)
        {
            scanning = false;

            Debug.Log("✅ QR FOUND: " + result.Text);

            OnQRDetected(result.Text);
        }
    }

    void OnQRDetected(string data)
    {
        Debug.Log("QR DATA: " + data);

        // Example:
        // LoadScene(data);
        // SpawnAnchor(data);
        // SendToSCORM(data);
    }

    void OnDestroy()
    {
        if (webcam != null && webcam.isPlaying)
            webcam.Stop();
    }
}
