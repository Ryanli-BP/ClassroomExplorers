using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
    private const int POINTS_PER_CORRECT_ANSWER = 2;
    private int correctAnswers = 0;

    public TextAsset csvFile;
    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = -1;
    public float quizDuration = 60f; // Duration of the quiz in seconds
    private float timeRemaining;
    private bool isQuizActive = false;

    private int currentPoints = 0;

    public static QuizManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject QuizUI; // Assign in inspector
    [SerializeField] private QuizDisplay QuizDisplay; // Assign in inspector
    [SerializeField] private AnswerButtons AnswerButtons; // Assign in inspector
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        if (QuizUI != null)
        {
            QuizUI.SetActive(false);
        }
        LoadQuestionsFromCSV();
    }
    

    public void StartNewQuiz()
    {
        if (questions == null || questions.Count == 0)
        {
            Debug.LogError("No questions loaded!");
            return;
        }

        // Enable UI
        QuizUI.SetActive(true);
        
        // Initialize quiz state
        timeRemaining = quizDuration;
        currentQuestionIndex = -1;
        currentPoints = 0;
        isQuizActive = true;
        
        if(AnswerButtons != null)
            AnswerButtons.EnableButtons();
        else
            Debug.LogError("AnswerButtons reference missing!");
            
        DisplayNextQuestion();
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



    private void EndQuiz()
    {
        isQuizActive = false;
        QuizUI.SetActive(false);
        Debug.Log("Quiz has ended!");
        currentPoints = correctAnswers * POINTS_PER_CORRECT_ANSWER;
        Player currPlayer = PlayerManager.Instance.GetCurrentPlayer();
        currPlayer.AddPoints(currentPoints);
        Debug.Log($"Player {currPlayer.getPlayerID()} scored {currentPoints} points!");
        currentPoints = 0;
        GameManager.Instance.OnQuizComplete();
    }


    public void DisplayNextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            Question currentQuestion = questions[currentQuestionIndex];
            QuizDisplay.DisplayQuestion(currentQuestion, currentQuestionIndex, questions.Count);
        }
    }
    public bool CheckAnswer(int answerIndex)
    {
        if (!isQuizActive) return false;

        string selectedAnswer = ((char)('A' + answerIndex)).ToString();
        return questions[currentQuestionIndex].answer == selectedAnswer;
    }

    public int GetCurrentPoints()
    {
        return currentPoints;
    }

    
    public void AnswerQuestion(bool isCorrect)
    {
        if (isCorrect)
        {
            correctAnswers++;
        }
        if (currentQuestionIndex >= questions.Count - 1)
        {
            EndQuiz();
        }
        else
        {
            DisplayNextQuestion();
        }
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