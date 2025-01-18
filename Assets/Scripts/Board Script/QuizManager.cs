using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

public class QuizManager : MonoBehaviour
{   
    [SerializeField] private GameObject QuizUI;
    public TextAsset csvFile;
    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = -1;
    public float quizDuration = 60f; // Duration of the quiz in seconds
    private float timeRemaining;
    private bool isQuizActive = false;
    private int correctAnswerCount = 0;

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

            if (timeRemaining <= 0 || currentQuestionIndex == questions.Count - 1)
            {
                EndQuiz();
            }
        }
    }

    public void StartNewQuiz()
    {
        if (questions == null || questions.Count == 0)
        {
            Debug.LogError("No questions loaded!");
            return;
        }

        QuizUI.SetActive(true);
        timeRemaining = quizDuration;
        currentQuestionIndex = -1;
        isQuizActive = true;
        
        // Reset and enable answers
        AnswerButtons.Instance.EnableButtons();
        
        // Show first question
        DisplayNextQuestion();
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
        QuizUI.SetActive(false);
        Debug.Log("Quiz has ended!");
        int pointsToAward = correctAnswerCount * 10;
        Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();

        correctAnswerCount = 0;
        Debug.Log($"Player {currentPlayer.getPlayerID()} scored {pointsToAward} points!");
        currentPlayer.AddPoints(pointsToAward);

        if (pointsToAward > 0)
        {
            UIManager.Instance.DisplayGainStarAnimation(currentPlayer.getPlayerID());
        }
        GameManager.Instance.HandleQuizEnd();
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

        if (selectedAnswer == questions[currentQuestionIndex].answer)
        {
            correctAnswerCount++;
        }
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