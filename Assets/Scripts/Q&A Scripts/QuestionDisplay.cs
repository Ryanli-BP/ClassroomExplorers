using UnityEngine;
using TMPro;

public class QuestionDisplay : MonoBehaviour
{
    public static QuestionDisplay Instance { get; private set; }

    [SerializeField] private GameObject ScreenQuestion;
    [SerializeField] private GameObject AnswerA;
    [SerializeField] private GameObject AnswerB;
    [SerializeField] private GameObject AnswerC;
    [SerializeField] private GameObject AnswerD;
    [SerializeField] private GameObject QuestionIndexText; 

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
}