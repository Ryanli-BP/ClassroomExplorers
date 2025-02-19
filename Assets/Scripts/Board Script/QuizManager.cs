using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;
using System.Linq;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private GameObject QuizUI;
    [SerializeField] private float slideSpeed = 1f;
    public TextAsset csvFile;
    private List<Question> questions = new List<Question>();
    private List<Question> availableQuestions = new List<Question>();
    private List<Question> usedQuestions = new List<Question>();
    private List<Question> currentQuizQuestions = new List<Question>();

    //Used for Buzz mode
    private float questionStartTime;
    private float lastAnswerTime;
    public float LastAnswerTime => lastAnswerTime;

    private int currentQuestionIndex = -1;
    private float quizDuration; // Duration of the quiz in seconds
    private int answeredQuestionsCount = 0;  
    private float timeRemaining;
    [SerializeField] private int timeRushQuestionCount = 5; // Number of questions to show per quiz

    private bool isQuizActive = false;
    private bool questionsLoaded = false;
    private bool isTransitioning = false;

    private int correctAnswerCount = 0;

    public bool OnQuizComplete { get; private set; }



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
        quizDuration = GameConfigManager.Instance.quizTimeLimit;
        StartCoroutine(PreloadQuestions());
        GameInitializer.Instance.ConfirmManagerReady("QuizManager");
    }

    private void Update()
    {
        if (isQuizActive)
        {
            timeRemaining -= Time.deltaTime;
            QuizDisplay.Instance.UpdateTimer(timeRemaining);
        }
    }

    private IEnumerator PreloadQuestions()
    {
        yield return StartCoroutine(DownloadQuestionCSV());
        availableQuestions = new List<Question>(questions);
        usedQuestions = new List<Question>();
        questionsLoaded = true;
        Debug.Log("Quiz questions preloaded successfully");
    }

    public void StartNewQuiz()
    {
        if (isTransitioning || isQuizActive) return;
        
        OnQuizComplete = false;
        UIManager.Instance.SetBoardUIActive(false);
        StartCoroutine(QuizSequence());
    }

    private IEnumerator QuizSequence()
    {
        yield return StartQuizSequence();
        StartQuizLogic();
        
        // Monitor quiz conditions
        while (isQuizActive)
        {
            if (timeRemaining <= 0 || answeredQuestionsCount >= questions.Count)
            {
                isQuizActive = false;
                Debug.Log("Quiz ended.");
                break;
            }
            yield return null;
        }
        
        yield return EndQuizSequence();
        HandleQuizComplete();
    }

    private IEnumerator StartQuizSequence()
    {
        isTransitioning = true;
        if (!questionsLoaded)
        {
            Debug.LogWarning("Questions not loaded yet, waiting...");
            yield return new WaitUntil(() => questionsLoaded);
        }

        if (questions == null || questions.Count == 0)
        {
            isTransitioning = false;
            yield break;
        }

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
    }

    private void StartQuizLogic()
    {
        timeRemaining = quizDuration;
        currentQuestionIndex = -1;
        isQuizActive = true;
        AnswerButtons.Instance.EnableButtons();
        
        // Get current quiz mode from GameConfigManager
        QuizMode currentQuizMode = GameConfigManager.Instance.CurrentQuizMode;
        
        // Determine number of questions based on quiz mode
        int questionCount = (currentQuizMode == QuizMode.TIME_RUSH) ? timeRushQuestionCount : 1;
        
        // Reset if we don't have enough available questions
        if (availableQuestions.Count < questionCount)
        {
            ResetQuestionPool();
        }
        
        // Create a separate list for this quiz session
        List<Question> selectedQuestions = availableQuestions
            .OrderBy(x => Random.value)
            .Take(questionCount)
            .ToList();
        
        // Remove selected questions from available pool
        foreach (var question in selectedQuestions)
        {
            availableQuestions.Remove(question);
            usedQuestions.Add(question);
        }
        
        // Store the quiz questions separately
        currentQuizQuestions = new List<Question>(selectedQuestions);
        DisplayNextQuestion();
    }


    private void ResetQuestionPool()
    {
        availableQuestions = new List<Question>(questions);
        usedQuestions.Clear();
        Debug.Log("Question pool has been reset");
    }
	
    public void DisplayNextQuestion()
    {
        if (!isQuizActive) return;

        // Check if we've shown all questions for this quiz session
        if (currentQuestionIndex + 1 >= currentQuizQuestions.Count)
        {
            isQuizActive = false;
            return;
        }

        currentQuestionIndex++;
        Question q = currentQuizQuestions[currentQuestionIndex];
        
        // Get current quiz mode from GameConfigManager
        QuizMode currentQuizMode = GameConfigManager.Instance.CurrentQuizMode;
        int totalQuestions = (currentQuizMode == QuizMode.TIME_RUSH) ? timeRushQuestionCount : 1;
        
        // Start timing when question is displayed
        questionStartTime = Time.time;
        
        QuizDisplay.Instance.DisplayQuestion(q, currentQuestionIndex + 1, totalQuestions);
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
    }

    private void HandleQuizComplete()
    {
        Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();

        QuizReward rewardTier = EvaluateQuizPerformance();
        Debug.Log($"{rewardTier}");
        RewardManager.Instance.GiveReward(rewardTier, currentPlayer);

        correctAnswerCount = 0;
        OnQuizComplete = true;
        UIManager.Instance.SetBoardUIActive(true);
    }

public QuizReward EvaluateQuizPerformance()
{
    QuizMode currentMode = GameConfigManager.Instance.CurrentQuizMode;
    
    switch (currentMode)
    {
        case QuizMode.NORMAL:
            return EvaluateDefaultMode();
        case QuizMode.BUZZ:
            return EvaluateBuzzMode();
        case QuizMode.TIME_RUSH:
            return EvaluateTimeRushMode();
        default:
            return QuizReward.NoReward;
    }
}

    private QuizReward EvaluateDefaultMode()
    {
        // For single question, check if it's correct
        if (correctAnswerCount == 0)
        {
            // 50/50 chance for small reward on incorrect answer
            return Random.value < 0.5f ? QuizReward.SmallReward : QuizReward.NoReward;
        }

        // Check player's quiz streak for bigger rewards
        int streak = PlayerManager.Instance.GetCurrentPlayer().QuizStreak;
        
        if (streak >= 5) return QuizReward.BigReward;
        if (streak >= 3) return QuizReward.MediumReward;
        return QuizReward.SmallReward;
    }

    private QuizReward EvaluateBuzzMode()
    {
        if (correctAnswerCount == 0)
        {
            // 50/50 chance for small reward on incorrect answer
            return Random.value < 0.5f ? QuizReward.SmallReward : QuizReward.NoReward;
        }

        // Calculate thresholds based on quiz duration
        float speedThresholdBig = quizDuration * 0.15f;    // 15% of time limit for big reward
        float speedThresholdMedium = quizDuration * 0.30f; // 30% of time limit for medium reward
        
        if (lastAnswerTime <= speedThresholdBig) return QuizReward.BigReward;
        if (lastAnswerTime <= speedThresholdMedium) return QuizReward.MediumReward;
        return QuizReward.SmallReward;
    }

    private QuizReward EvaluateTimeRushMode()
    {
        // Calculate net correct answers (correct - incorrect)
        int incorrectAnswers = answeredQuestionsCount - correctAnswerCount;
        int netScore = correctAnswerCount - incorrectAnswers;

        // No reward for zero or negative score
        if (netScore <= 0) return QuizReward.NoReward;

        // Big reward: net score of timeRushQuestionCount - 1 or better
        if (netScore >= timeRushQuestionCount - 1) return QuizReward.BigReward;

        // Medium reward: net score of 60% of timeRushQuestionCount or better
        float mediumThreshold = timeRushQuestionCount * 0.6f;
        if (netScore >= mediumThreshold) return QuizReward.MediumReward;

        // Small reward for any positive net score below medium threshold
        return QuizReward.SmallReward;
    }


    public bool CheckAnswer(int answerIndex)
    {
        if (!isQuizActive) return false;
        
        Question currentQuestion = currentQuizQuestions[currentQuestionIndex];
        string selectedAnswer = ((char)('A' + answerIndex)).ToString();
        answeredQuestionsCount++;

        // Calculate time taken to answer
        lastAnswerTime = Time.time - questionStartTime;

        bool isCorrect = selectedAnswer == currentQuestion.answer;
        Player currentPlayer = PlayerManager.Instance.GetCurrentPlayer();
        
        if (isCorrect)
        {
            correctAnswerCount++;
            currentPlayer.QuizStreak++;
            
            // Special handling for Buzz mode
            if (GameConfigManager.Instance.CurrentQuizMode == QuizMode.BUZZ)
            {
                Debug.Log($"Question answered in {lastAnswerTime:F2} seconds");
            }
        }
        else
        {
            currentPlayer.QuizStreak = 0; // Reset streak on wrong answer
        }
        
        return isCorrect;
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
            OLDLoadQuestionsFromCSV();
            //LoadQuestionsFromCSV(fallbackPath);
            yield break;
        }
    }

        private void OLDLoadQuestionsFromCSV() //used for testing on phone for now
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
            csv.Context.RegisterClassMap<QuestionMap>();
            questions = new List<Question>(csv.GetRecords<Question>());
        }

        Debug.Log($"Loaded {questions.Count} questions from the CSV.");
    }


    private void LoadQuestionsFromCSV(string filePath)
    {
        Debug.Log($"Attempting to load CSV from: {filePath}");
        Debug.Log($"File exists check: {File.Exists(filePath)}");
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