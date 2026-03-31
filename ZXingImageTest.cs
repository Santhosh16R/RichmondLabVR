using UnityEngine;
using ZXing;
using ZXing.Common;

public class ZXingImageTest : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Starting ZXing Image Test");

        Texture2D tex = Resources.Load<Texture2D>("testqr");

        if (tex == null)
        {
            Debug.LogError("❌ testqr.png not found");
            return;
        }

        var reader = new BarcodeReaderGeneric
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

        Color32[] pixels = tex.GetPixels32();

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
            tex.width,
            tex.height,
            RGBLuminanceSource.BitmapFormat.RGBA32
        );

        var result = reader.Decode(source);

        if (result != null)
            Debug.Log("✅ QR DETECTED: " + result.Text);
        else
            Debug.LogError("❌ QR NOT DETECTED");
    }
}
