using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeTeleport : MonoBehaviour
{
    [Header("References")]
    public Transform xrRig; // XR Origin or Camera Rig
    public Image fadeImage; // UI Image (Black)

    [Header("Settings")]
    public float fadeDuration = 1f;

    private Coroutine fadeRoutine;

    public void TeleportTo(Transform target)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeAndTeleport(target.position, target.rotation));
    }

    IEnumerator FadeAndTeleport(Vector3 targetPos, Quaternion targetRot)
    {
        // Fade Out
        yield return StartCoroutine(Fade(0, 1));

        // Move XR Rig
        xrRig.position = targetPos;
        xrRig.rotation = targetRot;

        // Fade In
        yield return StartCoroutine(Fade(1, 0));
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float time = 0f;
        Color color = fadeImage.color;

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);

            time += Time.deltaTime;
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}
