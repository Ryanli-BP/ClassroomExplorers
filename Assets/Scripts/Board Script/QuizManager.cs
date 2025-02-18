using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Newtonsoft.Json;

public class QuizManager : MonoBehaviour
{
    [SerializeField] private GameObject QuizUI;
    [SerializeField] private float slideSpeed = 1f;
    private RectTransform quizRect;
    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = -1;
    private float quizDuration = 5f; // Duration of the quiz in seconds
    private int answeredQuestionsCount = 0;  
    private float timeRemaining;
    private bool isQuizActive = false;
    private int correctAnswerCount = 0;
    private bool isTransitioning = false;
    public bool OnQuizComplete { get; private set; }
    private Player quizPlayer;
    private bool questionsLoaded = false;

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

    private void Start()
    {
        StartCoroutine(PreloadQuestions());
        GameInitializer.Instance.ConfirmManagerReady("QuizManager");
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

	// Preloads the quiz questions from the API
    private IEnumerator PreloadQuestions()
    {
        yield return StartCoroutine(DownloadQuestionsJSON());
        questionsLoaded = true;
        Debug.Log("Quiz questions preloaded successfully");
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

        QuizReward rewardTier = EvaluateQuizPerformance();
        Debug.Log($"{rewardTier}");
        RewardManager.Instance.GiveReward(rewardTier, currentPlayer);

        correctAnswerCount = 0;
        OnQuizComplete = true;
        UIManager.Instance.SetBoardUIActive(true);
    }

    public QuizReward EvaluateQuizPerformance()
    {
        return (QuizReward)Random.Range(0, 4);
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


	/*
	Handles the get of the quiz questions from the API.
	*/
	
	// Coroutine to download quiz questions in JSON format from the specified URL
	private IEnumerator DownloadQuestionsJSON() {
    	string url = "http://127.0.0.1:8000/api/v1.0.0/config/get-questions/";
    	string jsonResponse = null;
    	bool downloadCompleted = false;
	
    	// Use NetworkManager to download the text from the URL
    	NetworkManager.Instance.DownloadText(url,
        	(response) => { jsonResponse = response; downloadCompleted = true; },
        	(error) => { 
            	Debug.LogError($"Failed to download questions: {error}");
            	downloadCompleted = true;
        	});

    	// Wait until the download is completed
    	while (!downloadCompleted){
        	yield return null;
    	}

    	// If the response is not empty, parse the JSON
    	if (!string.IsNullOrEmpty(jsonResponse)) {
        	ParseQuestionsFromJSON(jsonResponse);
    	} else {
        	Debug.LogError("Failed to retrieve quiz questions from the API.");
    	}
	}

	// Method to parse the JSON response and load the questions
	private void ParseQuestionsFromJSON(string json) {
    	try {
        	var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, List<Question>>>(json);
        	if (jsonObject != null && jsonObject.ContainsKey("questions")) {
            	questions = jsonObject["questions"];
            	Debug.Log($"Loaded {questions.Count} questions from JSON response.");
        	} else {
            	Debug.LogError("JSON response does not contain 'questions' key.");
        	}
    	} catch (System.Exception e) {
        	Debug.LogError($"Error parsing JSON: {e.Message}");
    	}
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