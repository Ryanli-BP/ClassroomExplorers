using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtons : MonoBehaviour
{
    public static AnswerButtons Instance { get; private set; }
    [SerializeField] private GameObject[] answerButtons;
    [SerializeField] private AudioSource CorrectFX, WrongFX;

    private GameObject[] correctIndicators = new GameObject[4];  // Store "tick" objects
    private GameObject[] wrongIndicators = new GameObject[4];    // Store "wrong" objects

    private void Start()
    {
        AssignIndicators();
        SetupButtonListeners();
    }

    private void AssignIndicators()
    {
        // Assign the "tick" and "wrong" indicators within each button
        for (int i = 0; i < answerButtons.Length; i++)
        {
            correctIndicators[i] = answerButtons[i].transform.Find("tick").gameObject;   // Assuming "Tick" is the name of the GameObject
            wrongIndicators[i] = answerButtons[i].transform.Find("wrong").gameObject;   // Assuming "Wrong" is the name of the GameObject

            // Initially deactivate both indicators
            correctIndicators[i].SetActive(false);
            wrongIndicators[i].SetActive(false);
        }
    }

    private void SetupButtonListeners()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Capture index for lambda
            Button button = answerButtons[i].GetComponent<Button>();
            button.onClick.AddListener(() => OnAnswerSelected(index));
            button.interactable = true;  // Ensure button is clickable
        }
    }

    public void OnAnswerSelected(int answerIndex)
    {
        DisableAllButtons();
        bool isCorrect = QuizManager.Instance.CheckAnswer(answerIndex);

        // Activate the appropriate indicator (Tick or Wrong)
        if (isCorrect)
        {
            correctIndicators[answerIndex].SetActive(true);  // Activate tick
            if (CorrectFX != null)
            {
                CorrectFX.Play();
            }
        }
        else
        {
            wrongIndicators[answerIndex].SetActive(true);    // Activate wrong
            if (WrongFX != null)
            {
                WrongFX.Play();
            }
        }

        StartCoroutine(NextQuestion());
    }

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

    public void EnableButtons()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponent<Button>().enabled = true;
            correctIndicators[i].SetActive(false); // Reset tick indicator
            wrongIndicators[i].SetActive(false);   // Reset wrong indicator
        }
    }

    public void SelectAnswer(int answerIndex)
    {
        if (!QuizManager.Instance.IsQuizActive()) return;

        bool isCorrect = QuizManager.Instance.CheckAnswer(answerIndex);

        // Activate the appropriate indicator (Tick or Wrong)
        if (isCorrect)
        {
            correctIndicators[answerIndex].SetActive(true);  // Activate tick
            CorrectFX.Play();
        }
        else
        {
            wrongIndicators[answerIndex].SetActive(true);    // Activate wrong
            WrongFX.Play();
        }

        DisableAllButtons();
        StartCoroutine(NextQuestion());
    }

    private void DisableAllButtons()
    {
        foreach (GameObject button in answerButtons)
        {
            button.GetComponent<Button>().enabled = false;
        }
    }

    private IEnumerator NextQuestion()
    {
        yield return new WaitForSeconds(2.0f);

        // Reset all indicators before the next question
        for (int i = 0; i < answerButtons.Length; i++)
        {
            correctIndicators[i].SetActive(false); // Deactivate tick
            wrongIndicators[i].SetActive(false);   // Deactivate wrong
            answerButtons[i].GetComponent<Button>().enabled = true;
        }

        QuizManager.Instance.DisplayNextQuestion();
    }
}
