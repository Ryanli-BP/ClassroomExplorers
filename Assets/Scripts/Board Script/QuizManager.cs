using System.Collections;
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
    private bool isTransitioning = false;
    public bool OnQuizComplete { get; private set; }
    private Player quizPlayer;

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

            if (timeRemaining <= 0 || answeredQuestionsCount >= questions.Count)
            {
                EndQuiz();
            }
        }
    }

    public void StartNewQuiz()
    {
        if (isTransitioning || isQuizActive) return;
        
        OnQuizComplete = false;
        UIManager.Instance.SetBoardUIActive(false);
        StartCoroutine(StartQuizSequence());
    }

    private IEnumerator StartQuizSequence()
    {
        isTransitioning = true;
        yield return StartCoroutine(DownloadQuestionCSV());

        if (questions == null || questions.Count == 0)
        {
            isTransitioning = false;
            yield break;
        }

        quizPlayer = PlayerManager.Instance.GetCurrentPlayer();
        answeredQuestionsCount = 0;
        currentQuestionIndex = -1;
        correctAnswerCount = 0;
        timeRemaining = quizDuration;

        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.SetBoardUIActive(false);

        yield return new WaitForSeconds(0.2f);
        QuizUI.SetActive(true);

        LeanTween.moveLocalY(QuizUI, -540f, slideSpeed)
            .setEase(LeanTweenType.easeOutBack);

        yield return new WaitForSeconds(slideSpeed);
        isQuizActive = true;
        isTransitioning = false;
        StartQuizLogic();
    }

	private void StartQuizLogic()
    {
    	timeRemaining = quizDuration;
    	currentQuestionIndex = -1;
    	isQuizActive = true;
    	AnswerButtons.Instance.EnableButtons();
    	DisplayNextQuestion();
    }
	
	private IEnumerator EndQuizSequence()
    {
        isTransitioning = true;
        isQuizActive = false;
        
        LeanTween.moveLocalY(QuizUI, -1080f, slideSpeed)
            .setEase(LeanTweenType.easeInBack);
            
        yield return new WaitForSeconds(slideSpeed);
        QuizUI.SetActive(false);
        
        yield return new WaitForSeconds(0.2f);
        UIManager.Instance.SetBoardUIActive(true);
        
        yield return new WaitForSeconds(0.1f);
        isTransitioning = false;
        HandleQuizComplete();
    }

    private void EndQuiz()
    {
        if (isTransitioning) return;
        Debug.Log("Quiz ended.");
        StartCoroutine(EndQuizSequence());
    }

    private void HandleQuizComplete()
    {
        int pointsToAward = correctAnswerCount * 10;
        Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();
        correctAnswerCount = 0;

        if (pointsToAward > 0)
        {
            currentPlayer.AddPoints(pointsToAward);
            UIManager.Instance.DisplayPointChange(pointsToAward);
            UIManager.Instance.DisplayGainStarAnimation(currentPlayer.getPlayerID());
        }

        OnQuizComplete = true;
        UIManager.Instance.SetBoardUIActive(true);
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

        private IEnumerator DownloadQuestionCSV()
    {
        string url = "http://127.0.0.1:8000/api/v1.0.0/config/get-questions/";
        string savePath = Path.Combine(Application.persistentDataPath, "QuestionCSV.csv");
        string fallbackPath = Path.Combine(Application.dataPath, "Questions CSV", "questionsTest.csv");

        bool downloadCompleted = false;
        string errorMessage = null;

        NetworkManager.Instance.DownloadFile(url, savePath, 
            () => { downloadCompleted = true; }, 
            (error) => { errorMessage = error; downloadCompleted = true; });

        while (!downloadCompleted)
        {
            yield return null;
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Debug.LogWarning($"Failed to download questions: {errorMessage}. Falling back to local CSV file.");
            LoadQuestionsFromCSV(fallbackPath);
            yield break;
        }

        LoadQuestionsFromCSV(savePath);
    }

    private void LoadQuestionsFromCSV(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found!");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<QuestionMap>();
            questions = new List<Question>(csv.GetRecords<Question>());
        }

        Debug.Log($"Loaded {questions.Count} questions from the CSV.");
    }
}

public class Question
{
    public string question { get; set; }
    public string choiceA { get; set; }
    public string choiceB { get; set; }
    public string choiceC { get; set; }
    public string choiceD { get; set; }
    public string answer { get; set; }
}

public sealed class QuestionMap : ClassMap<Question>
{
    public QuestionMap()
    {
        Map(m => m.question);
        Map(m => m.choiceA);
        Map(m => m.choiceB);
        Map(m => m.choiceC);
        Map(m => m.choiceD);
        Map(m => m.answer);
    }
}