using UnityEngine;
using System;
using System.Collections;

public class MCQManager : MonoBehaviour
{
    [Header("Questions")]
    public MCQQuestion[] questions;

    [Header("Audio")]
    public MCQAudioManager audioManager;

    int currentQuestionIndex = 0;

    public int TotalScore { get; private set; }
    public bool IsQuizRunning { get; private set; }

    bool waitingForNext = false;

    // ---------------- EVENTS ----------------

    public Action<MCQQuestion> OnQuestionLoaded;

    public Action<bool, int, int, string> OnAnswerFeedback;

    public Action<int> OnQuizCompleted;

    // ---------------- REPORT ----------------

    float quizStartTime;
    float questionStartTime;

    MCQReportData report = new MCQReportData();

    // -------------------------------------------------
    // START QUIZ
    // -------------------------------------------------
    public void StartQuiz()
    {
        if (questions.Length == 0)
        {
            Debug.LogError("No Questions Assigned!");
            return;
        }

        TotalScore = 0;
        currentQuestionIndex = 0;

        report = new MCQReportData();

        quizStartTime = Time.time;

        IsQuizRunning = true;

        LoadQuestion();
    }

    // -------------------------------------------------
    // LOAD QUESTION
    // -------------------------------------------------
    void LoadQuestion()
    {
        waitingForNext = false;

        if (currentQuestionIndex >= questions.Length)
        {
            FinishQuiz();
            return;
        }

        MCQQuestion q = questions[currentQuestionIndex];

        questionStartTime = Time.time;

        OnQuestionLoaded?.Invoke(q);

        if (audioManager != null)
            audioManager.Play(q.questionAudio);
    }

    // -------------------------------------------------
    // SUBMIT ANSWER
    // -------------------------------------------------
    public void SubmitAnswer(int selectedIndex)
    {
        if (!IsQuizRunning || waitingForNext)
            return;

        StartCoroutine(HandleAnswer(selectedIndex));
    }

    // -------------------------------------------------
    // HANDLE ANSWER
    // -------------------------------------------------
    IEnumerator HandleAnswer(int selectedIndex)
    {
        MCQQuestion q = questions[currentQuestionIndex];

        bool isCorrect = selectedIndex == q.correctAnswerIndex;

        if (isCorrect)
            TotalScore += q.marks;

        float timeTaken = Time.time - questionStartTime;

        // Save report data
        QuestionReport qr = new QuestionReport();
        qr.questionText = q.question;
        qr.isCorrect = isCorrect;
        qr.timeTaken = timeTaken;

        report.questionReports.Add(qr);

        string feedbackText =
            isCorrect ? q.correctFeedbackText
                      : q.wrongFeedbackText;

        OnAnswerFeedback?.Invoke(
            isCorrect,
            q.correctAnswerIndex,
            TotalScore,
            feedbackText
        );

        // Play feedback audio
        if (audioManager != null)
        {
            if (isCorrect)
                audioManager.Play(q.correctFeedbackAudio);
            else
                audioManager.Play(q.wrongFeedbackAudio);
        }

        // Wait for audio finish
        if (audioManager != null)
        {
            while (audioManager.IsPlaying())
                yield return null;
        }

        // NOW WAIT FOR BUTTON CLICK
        waitingForNext = true;
    }

    // -------------------------------------------------
    // NEXT QUESTION BUTTON
    // -------------------------------------------------
    public void NextQuestion()
    {
        if (!waitingForNext)
            return;

        currentQuestionIndex++;

        LoadQuestion();
    }

    // -------------------------------------------------
    // FINISH QUIZ
    // -------------------------------------------------
    void FinishQuiz()
    {
        IsQuizRunning = false;

        report.totalScore = TotalScore;
        report.totalTime = Time.time - quizStartTime;
        report.percentage = GetPercentage();

        OnQuizCompleted?.Invoke(TotalScore);

        Debug.Log("Quiz Completed");
    }

    // -------------------------------------------------
    // HELPERS
    // -------------------------------------------------
    public float GetPercentage()
    {
        int maxScore = 0;

        foreach (var q in questions)
            maxScore += q.marks;

        if (maxScore == 0) return 0;

        return (float)TotalScore / maxScore * 100f;
    }

    public MCQReportData GetReport()
    {
        return report;
    }
}
