using UnityEngine;
using ZXing;
using ZXing.Common;
using System.Collections;

public class MetaOpenXRQRScanner : MonoBehaviour
{
    WebCamTexture webcam;
    BarcodeReaderGeneric reader;

    bool scanning = false;

    IEnumerator Start()
    {
        Debug.Log("Requesting Camera Permission...");

        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("❌ Camera permission denied");
            yield break;
        }

        //----------------------------------
        // FIND QUEST CAMERA
        //----------------------------------
        WebCamDevice[] devices = WebCamTexture.devices;

        foreach (var d in devices)
            Debug.Log("Camera Found: " + d.name);

        string camName = null;

        foreach (var d in devices)
        {
            if (d.name.ToLower().Contains("camera2"))
            {
                camName = d.name;
                break;
            }
        }

        if (camName == null)
        {
            Debug.LogError("❌ camera2 not found");
            yield break;
        }

        //----------------------------------
        // START CAMERA
        //----------------------------------
        webcam = new WebCamTexture(camName, 1280, 720, 30);
        webcam.Play();

        Debug.Log("Starting camera...");

        yield return new WaitForSeconds(2f);

        //----------------------------------
        // ZXING SETUP
        //----------------------------------
        reader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = new[]
                {
                    BarcodeFormat.QR_CODE
                }
            }
        };

        scanning = true;

        Debug.Log("✅ QR Scanner Ready");
    }

    void Update()
    {
        if (!scanning || webcam == null)
            return;

        if (webcam.width < 100)
            return;

        ScanQR();
    }

    void ScanQR()
    {
        Color32[] pixels = webcam.GetPixels32();

        byte[] raw = new byte[pixels.Length * 4];

        for (int i = 0; i < pixels.Length; i++)
        {
            raw[i * 4 + 0] = pixels[i].r;
            raw[i * 4 + 1] = pixels[i].g;
            raw[i * 4 + 2] = pixels[i].b;
            raw[i * 4 + 3] = pixels[i].a;
        }

        var source = new RGBLuminanceSource(
            raw,
            webcam.width,
            webcam.height,
            RGBLuminanceSource.BitmapFormat.RGBA32
        );

        var result = reader.Decode(source);

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

        // Example actions
        // SceneManager.LoadScene(data);
        // Open URL
        Application.OpenURL(data);
    }
}
