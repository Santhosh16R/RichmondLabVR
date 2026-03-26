using UnityEngine;

[CreateAssetMenu(fileName = "MCQ_Question", menuName = "MCQ/Question")]
public class MCQQuestion : ScriptableObject
{
    [TextArea(2,5)]
    public string question;

    public string[] options = new string[4];
    public int correctAnswerIndex;

    [Header("Scoring")]
    public int marks = 1;

    [Header("Audio")]
    public AudioClip questionAudio;
    public AudioClip correctFeedbackAudio;
    public AudioClip wrongFeedbackAudio;

    [Header("Text Feedback")]
    [TextArea(2,5)]
    public string correctFeedbackText;

    [TextArea(2,5)]
    public string wrongFeedbackText;
}
