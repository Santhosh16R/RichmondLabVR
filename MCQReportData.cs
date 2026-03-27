using System;
using System.Collections.Generic;

[Serializable]
public class QuestionReport
{
    public string questionText;
    public bool isCorrect;
    public float timeTaken;
}

[Serializable]
public class MCQReportData
{
    public int totalScore;
    public float percentage;
    public float totalTime;

    public List<QuestionReport> questionReports =
        new List<QuestionReport>();
}
