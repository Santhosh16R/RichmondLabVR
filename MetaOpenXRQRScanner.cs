using UnityEngine;
using ZXing;
using System.Collections;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Meta;

public class MetaOpenXRQRScanner : MonoBehaviour
{
    private BarcodeReader reader;
    private bool scanning = false;

    Texture2D cameraTexture;

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

        reader = new BarcodeReader
        {
            AutoRotate = true,
            TryInverted = true
        };

        scanning = true;

        Debug.Log("QR Scanner Started");
    }

    void Update()
    {
        if (!scanning)
            return;

        TryScanQR();
    }

    void TryScanQR()
    {
        // Get latest Meta camera frame
        if (!MetaOpenXRCamera.TryGetLatestFrame(out var frame))
            return;

        int width = frame.width;
        int height = frame.height;

        if (cameraTexture == null)
            cameraTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        cameraTexture.LoadRawTextureData(frame.pixelData);
        cameraTexture.Apply();

        var result = reader.Decode(
            cameraTexture.GetPixels32(),
            width,
            height
        );

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

        // Example usage
        // SceneManager.LoadScene(data);
        // SendToServer(data);
        // SpawnMRContent(data);
    }
}
