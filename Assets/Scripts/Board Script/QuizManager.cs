using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

public class QuizManager : MonoBehaviour
{   
    [SerializeField] private GameObject QuizUI;
    [SerializeField] private float slideSpeed = 1f;
    private RectTransform quizRect;
    public TextAsset csvFile;
    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = -1;
    public float quizDuration = 60f; // Duration of the quiz in seconds
    private int answeredQuestionsCount = 0;  
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
            quizRect = QuizUI.GetComponent<RectTransform>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isQuizActive)
        {
            timeRemaining -= Time.deltaTime;
            QuizDisplay.Instance.UpdateTimer(timeRemaining); // Update the timer display

            if (timeRemaining <= 0 || answeredQuestionsCount >= questions.Count )
            {
                EndQuiz();
            }
        }
    }
    public void StartNewQuiz()
    {
        LoadQuestionsFromCSV();
        if (questions == null || questions.Count == 0) return;
        
        UIManager.Instance.SetBoardUIActive(false);
        QuizUI.SetActive(true);
        
        Vector3 finalPosition = new Vector3(QuizUI.transform.localPosition.x, -540f, QuizUI.transform.localPosition.z);
        
        LeanTween.moveLocalY(QuizUI, -540f, slideSpeed)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {
                QuizUI.transform.localPosition = finalPosition;  // Lock position
                StartQuizLogic();
            });
    }

    private void StartQuizLogic()
    {
    timeRemaining = quizDuration;
    currentQuestionIndex = -1;
    isQuizActive = true;
    AnswerButtons.Instance.EnableButtons();
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


    private void EndQuiz()
    {
        isQuizActive = false;
        
        LeanTween.moveLocalY(QuizUI, -1080f, slideSpeed)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => {
                QuizUI.SetActive(false);
                HandleQuizComplete();
            });
    }

    private void HandleQuizComplete()
    {
    int pointsToAward = correctAnswerCount * 10;
    Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();
    correctAnswerCount = 0;
    
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
        answeredQuestionsCount++;
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