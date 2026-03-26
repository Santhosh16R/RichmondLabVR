using UnityEngine;
using TMPro;

public class MCQUIHandler : MonoBehaviour
{
    public MCQManager manager;

    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreText;

    public MCQOptionButton[] options;

    void OnEnable()
    {
        manager.OnQuestionLoaded += DisplayQuestion;
        manager.OnAnswerFeedback += ShowFeedback;
    }

    void OnDisable()
    {
        manager.OnQuestionLoaded -= DisplayQuestion;
        manager.OnAnswerFeedback -= ShowFeedback;
    }

    void DisplayQuestion(MCQQuestion q)
    {
        feedbackText.text = "";

        questionText.text = q.question;

        for (int i = 0; i < options.Length; i++)
            options[i].Setup(q.options[i], i, manager);
    }

    void ShowFeedback(bool correct,
                      int correctIndex,
                      int score,
                      string feedback)
    {
        scoreText.text = "Score : " + score;

        feedbackText.text =
            feedback +
            "\n\nCorrect Answer: " +
            options[correctIndex].GetText();
    }
}
