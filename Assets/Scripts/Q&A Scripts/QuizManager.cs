using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    public TextAsset csvFile;
    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = -1;
    public float quizDuration = 60f; // Duration of the quiz in seconds
    private float timeRemaining;
    private bool isQuizActive = false;


    private List<Question> originalQuestions;

    public void StartQuizInteraction(bool isBuzzTile)
    {
        if (originalQuestions == null)
        {
            originalQuestions = new List<Question>(questions); // Backup original questions
        }

        if (isBuzzTile)
        {
            Debug.Log("Buzz tile: Starting single-question interaction.");
            quizDuration = 10f; // Shorter duration for Buzz
            questions = new List<Question> { originalQuestions[UnityEngine.Random.Range(0, originalQuestions.Count)] }; // Select one random question
        }
        else
        {
            Debug.Log("Quiz tile: Starting full quiz interaction.");
            quizDuration = 60f; // Standard duration for Quiz
            questions = new List<Question>(originalQuestions); // Restore original questions
        }

        StartQuiz();
    }

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
        StartQuiz();
    }


    private void Update()
    {
        if (isQuizActive)
        {
            timeRemaining -= Time.deltaTime;
            QuizDisplay.Instance.UpdateTimer(timeRemaining); // Update the timer display

            if (timeRemaining <= 0)
            {
                EndQuiz();
            }
        }
    }

    private void LoadQuestionsFromCSV()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not assigned in the inspector!");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using (var reader = new StringReader(csvFile.text))
        using (var csv = new CsvReader(reader, config))
        {
            questions = new List<Question>(csv.GetRecords<Question>());
        }

        Debug.Log($"Loaded {questions.Count} questions from the CSV.");
    }

    private void StartQuiz()
    {
        timeRemaining = quizDuration;
        isQuizActive = true;
        DisplayNextQuestion();
    }

    private void EndQuiz()
    {
        isQuizActive = false;
        Debug.Log("Quiz has ended!");
        // Additional logic to handle the end of the quiz can be added here
    }

    public void DisplayNextQuestion()
    {
        if (!isQuizActive) return;

        currentQuestionIndex = (currentQuestionIndex + 1) % questions.Count;
        Question q = questions[currentQuestionIndex];

        QuizDisplay.Instance.DisplayQuestion(q, currentQuestionIndex, questions.Count);
    }

    public bool CheckAnswer(int answerIndex)
    {
        if (!isQuizActive) return false;

        string selectedAnswer = ((char)('A' + answerIndex)).ToString();
        return questions[currentQuestionIndex].answer == selectedAnswer;
    }

    public bool IsQuizActive()
    {
        return isQuizActive;
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