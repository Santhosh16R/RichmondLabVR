using UnityEngine;

public class MCQAudioManager : MonoBehaviour
{
    public AudioSource voiceSource;

    public void Play(AudioClip clip)
    {
        if (clip == null) return;

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();
    }

    public bool IsPlaying()
    {
        return voiceSource.isPlaying;
    }
}
