using UnityEngine;
using TMPro;

public class MCQUIHandler : MonoBehaviour
{
    public MCQManager manager;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI scoreText;
    public MCQOptionButton[] optionButtons;

    void OnEnable()
    {
        manager.OnQuestionLoaded += DisplayQuestion;
        manager.OnAnswerResult += ShowResult;
        manager.OnQuizCompleted += ShowFinalScore;
    }

    void OnDisable()
    {
        manager.OnQuestionLoaded -= DisplayQuestion;
        manager.OnAnswerResult -= ShowResult;
        manager.OnQuizCompleted -= ShowFinalScore;
    }

    void DisplayQuestion(MCQQuestion q)
    {
        questionText.text = q.question;

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].Setup(q.options[i], i, manager);
        }
    }

    void ShowResult(bool isCorrect, int score)
    {
        scoreText.text = "Score: " + score;

        Debug.Log(isCorrect ? "Correct!" : "Wrong!");
    }

    void ShowFinalScore(int finalScore)
    {
        Debug.Log("Quiz Completed! Final Score: " + finalScore);
    }
}
