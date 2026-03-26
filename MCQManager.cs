using UnityEngine;
using System;
using System.Collections;

public class MCQManager : MonoBehaviour
{
    [Header("Questions")]
    public MCQQuestion[] questions;

    [Header("Audio")]
    public MCQAudioManager audioManager;

    // -----------------------------
    // Runtime Data
    // -----------------------------
    int currentQuestionIndex = 0;

    public int TotalScore { get; private set; }

    public bool IsQuizRunning { get; private set; }

    // -----------------------------
    // Events (UI / VR / SCORM)
    // -----------------------------
    public Action<MCQQuestion> OnQuestionLoaded;

    // bool isCorrect
    // int correctIndex
    // int totalScore
    // string feedbackText
    public Action<bool, int, int, string> OnAnswerFeedback;

    public Action<int> OnQuizCompleted;

    // -------------------------------------------------
    // START QUIZ
    // -------------------------------------------------
    public void StartQuiz()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogError("No MCQ Questions Assigned!");
            return;
        }

        TotalScore = 0;
        currentQuestionIndex = 0;
        IsQuizRunning = true;

        LoadQuestion();
    }

    // -------------------------------------------------
    // LOAD QUESTION
    // -------------------------------------------------
    void LoadQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            FinishQuiz();
            return;
        }

        MCQQuestion q = questions[currentQuestionIndex];

        OnQuestionLoaded?.Invoke(q);

        // Play question narration
        if (audioManager != null)
            audioManager.Play(q.questionAudio);
    }

    // -------------------------------------------------
    // SUBMIT ANSWER
    // -------------------------------------------------
    public void SubmitAnswer(int selectedIndex)
    {
        if (!IsQuizRunning) return;

        StartCoroutine(HandleAnswer(selectedIndex));
    }

    // -------------------------------------------------
    // ANSWER PROCESS FLOW
    // -------------------------------------------------
    IEnumerator HandleAnswer(int selectedIndex)
    {
        MCQQuestion q = questions[currentQuestionIndex];

        bool isCorrect = selectedIndex == q.correctAnswerIndex;

        // Add Score
        if (isCorrect)
            TotalScore += q.marks;

        // Choose feedback text
        string feedbackText =
            isCorrect ? q.correctFeedbackText
                      : q.wrongFeedbackText;

        // Notify UI / VR system
        OnAnswerFeedback?.Invoke(
            isCorrect,
            q.correctAnswerIndex,
            TotalScore,
            feedbackText
        );

        // Play feedback voice
        if (audioManager != null)
        {
            if (isCorrect)
                audioManager.Play(q.correctFeedbackAudio);
            else
                audioManager.Play(q.wrongFeedbackAudio);
        }

        // Wait for voice to finish
        if (audioManager != null)
        {
            while (audioManager.IsPlaying())
                yield return null;
        }

        // Small delay before next question
        yield return new WaitForSeconds(1f);

        currentQuestionIndex++;

        LoadQuestion();
    }

    // -------------------------------------------------
    // QUIZ FINISH
    // -------------------------------------------------
    void FinishQuiz()
    {
        IsQuizRunning = false;

        Debug.Log("Quiz Completed. Final Score: " + TotalScore);

        OnQuizCompleted?.Invoke(TotalScore);
    }

    // -------------------------------------------------
    // HELPERS
    // -------------------------------------------------

    public int GetCurrentQuestionNumber()
    {
        return currentQuestionIndex + 1;
    }

    public int GetTotalQuestions()
    {
        return questions.Length;
    }

    public float GetPercentage()
    {
        int maxScore = 0;

        foreach (var q in questions)
            maxScore += q.marks;

        if (maxScore == 0) return 0;

        return (float)TotalScore / maxScore * 100f;
    }

    public void StopQuiz()
    {
        StopAllCoroutines();
        IsQuizRunning = false;
    }
}
