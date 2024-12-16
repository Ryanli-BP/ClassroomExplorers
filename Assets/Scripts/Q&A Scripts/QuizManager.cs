using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public TextAsset csvFile;
    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = -1;

    public static QuizManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadQuestionsFromCSV();
        DisplayNextQuestion();
    }

    private void LoadQuestionsFromCSV()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned in the inspector!");
            return;
        }

        using (var reader = new StringReader(csvFile.text))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            questions = new List<Question>(csv.GetRecords<Question>());
        }

        Debug.Log($"Loaded {questions.Count} questions from the CSV.");
    }

    public void DisplayNextQuestion()
    {
        currentQuestionIndex = (currentQuestionIndex + 1) % questions.Count;
        Question q = questions[currentQuestionIndex];

        QuestionDisplay.Instance.DisplayQuestion(q, currentQuestionIndex, questions.Count);
    }

    public bool CheckAnswer(int answerIndex)
    {
        string selectedAnswer = ((char)('A' + answerIndex)).ToString();
        return questions[currentQuestionIndex].answer == selectedAnswer;
    }
}

public class Question
{
    public string question { get; set; }
    public string optionA { get; set; }
    public string optionB { get; set; }
    public string optionC { get; set; }
    public string optionD { get; set; }
    public string answer { get; set; }
}