using UnityEngine;
using TMPro;
using static QuizManager;  

public class QuizDisplay : MonoBehaviour
{
    public static QuizDisplay Instance { get; private set; }

    [SerializeField] private GameObject ScreenQuestion;
    [SerializeField] private GameObject AnswerA;
    [SerializeField] private GameObject AnswerB;
    [SerializeField] private GameObject AnswerC;
    [SerializeField] private GameObject AnswerD;
    [SerializeField] private GameObject QuestionIndexText;
    [SerializeField] private GameObject TimerText; // Add a new GameObject for displaying the timer

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

    public void DisplayQuestion(Question question, int currentIndex, int totalQuestions)
    {
        ScreenQuestion.GetComponent<TextMeshProUGUI>().text = question.question;
        AnswerA.GetComponent<TextMeshProUGUI>().text = $"A. {question.optionA}";
        AnswerB.GetComponent<TextMeshProUGUI>().text = $"B. {question.optionB}";
        AnswerC.GetComponent<TextMeshProUGUI>().text = $"C. {question.optionC}";
        AnswerD.GetComponent<TextMeshProUGUI>().text = $"D. {question.optionD}";
        QuestionIndexText.GetComponent<TextMeshProUGUI>().text = $"{currentIndex + 1}/{totalQuestions}"; // Display the index
    }

    public void UpdateTimer(float timeRemaining)
    {
        TimerText.GetComponent<TextMeshProUGUI>().text = $"Time: {timeRemaining:F2}"; // Display the timer
    }
}