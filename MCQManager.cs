using UnityEngine;
using System;

public class MCQManager : MonoBehaviour
{
    public MCQQuestion[] questions;

    private int currentQuestionIndex = 0;

    public int TotalScore { get; private set; } = 0;

    public Action<MCQQuestion> OnQuestionLoaded;
    public Action<bool, int> OnAnswerResult; // (isCorrect, currentScore)
    public Action<int> OnQuizCompleted; // final score

    public void StartQuiz()
    {
        currentQuestionIndex = 0;
        TotalScore = 0;
        LoadQuestion();
    }

    void LoadQuestion()
    {
        if (currentQuestionIndex < questions.Length)
        {
            OnQuestionLoaded?.Invoke(questions[currentQuestionIndex]);
        }
        else
        {
            OnQuizCompleted?.Invoke(TotalScore);
        }
    }

    public void SubmitAnswer(int selectedIndex)
    {
        var current = questions[currentQuestionIndex];

        bool isCorrect = selectedIndex == current.correctAnswerIndex;

        if (isCorrect)
        {
            TotalScore += current.marks;
        }

        OnAnswerResult?.Invoke(isCorrect, TotalScore);

        currentQuestionIndex++;

        Invoke(nameof(LoadQuestion), 1.5f);
    }
}
