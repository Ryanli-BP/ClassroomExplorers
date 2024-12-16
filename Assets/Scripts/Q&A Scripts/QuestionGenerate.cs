using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class QuestionGenerate : MonoBehaviour
{
    public static string ActualAnswer;
    public static bool displayingQuestion = false;

    // CSV file (drag and drop in the Unity inspector)
    public TextAsset csvFile;

    // Internal question structure
    private class Question
    {
        public string question;
        public string optionA;
        public string optionB;
        public string optionC;
        public string optionD;
        public string answer;
    }

    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = -1;

    void Start()
    {
        LoadQuestionsFromCSV();
    }

    void Update()
    {
        if (displayingQuestion == false && questions.Count > 0)
        {
            displayingQuestion = true;
            DisplayNextQuestion();

            QuestionDisplay.UpdateNow = false;
        }
    }

    void LoadQuestionsFromCSV()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned in the inspector!");
            return;
        }

        using (StringReader reader = new StringReader(csvFile.text))
        {
            string line;
            bool isFirstLine = true;
            while ((line = reader.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    isFirstLine = false; // Skip header
                    continue;
                }

                string[] fields = line.Split(',');

                if (fields.Length == 6) // Ensure the line has the correct number of fields
                {
                    Question q = new Question
                    {
                        question = fields[0],
                        optionA = fields[1],
                        optionB = fields[2],
                        optionC = fields[3],
                        optionD = fields[4],
                        answer = fields[5]
                    };

                    questions.Add(q);
                }
            }
        }

        Debug.Log($"Loaded {questions.Count} questions from the CSV.");
    }

    void DisplayNextQuestion()
    {
        currentQuestionIndex = (currentQuestionIndex + 1) % questions.Count;
        Question q = questions[currentQuestionIndex];

        QuestionDisplay.NewQuestion = q.question;
        QuestionDisplay.NewA = $"A. {q.optionA}";
        QuestionDisplay.NewB = $"B. {q.optionB}";
        QuestionDisplay.NewC = $"C. {q.optionC}";
        QuestionDisplay.NewD = $"D. {q.optionD}";
        ActualAnswer = q.answer;
    }
}
