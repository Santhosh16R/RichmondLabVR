using UnityEngine;
using Whisper;
using System.Threading.Tasks;

public class WhisperUIButtonRecorder : MonoBehaviour
{
    public WhisperManager whisper;
    public QuestTTS tts;

    private AudioClip recordingClip;
    private string micName;
    private bool isRecording = false;

    void Start()
    {
        micName = Microphone.devices[0];
    }

    // BUTTON PRESS
    public void StartRecording()
    {
        if (isRecording) return;

        Debug.Log("Recording Started");

        recordingClip = Microphone.Start(
            micName,
            false,
            30,
            16000
        );

        isRecording = true;
    }

    // BUTTON RELEASE
    public async void StopRecording()
    {
        if (!isRecording) return;

        Debug.Log("Recording Stopped");

        Microphone.End(micName);
        isRecording = false;

        await Transcribe(recordingClip);
    }

    async Task Transcribe(AudioClip clip)
    {
        var result = await whisper.GetTextAsync(clip);

        Debug.Log("USER SAID: " + result);

        tts.Speak(result);
    }
}
