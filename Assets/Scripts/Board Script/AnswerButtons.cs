using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtons : MonoBehaviour
{
    public static AnswerButtons Instance { get; private set; }
    [SerializeField] private GameObject[] answerButtons;
    [SerializeField] private AudioSource CorrectFX, WrongFX;

    private Color[] defaultColors = new Color[4]; // Store unique default colors
    private Color correctColor = Color.green;  // Color for correct answer
    private Color wrongColor = Color.red; 

    private void Start()
    {
        AssignDefaultColors();
        SetupButtonListeners();
    }

    private void AssignDefaultColors()
    {
        // Define unique colors that are different from green and red
        defaultColors[0] = new Color(0.6f, 0.8f, 1f); // Light Blue
        defaultColors[1] = new Color(1f, 0.8f, 0.2f); // Yellow-Orange
        defaultColors[2] = new Color(1f, 0.6f, 0.8f); // Pink
        defaultColors[3] = new Color(0.8f, 0.8f, 0.8f); // Light Gray

        // Apply colors to buttons
        for (int i = 0; i < answerButtons.Length; i++)
        {
            Image buttonImage = answerButtons[i].GetComponent<Image>();
            buttonImage.color = defaultColors[i];
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

        Image buttonImage = answerButtons[answerIndex].GetComponent<Image>(); // Get the button image

        if (isCorrect)
        {
            buttonImage.color = correctColor;
            if (CorrectFX != null)
            {
                CorrectFX.Play();
            }
        }
        else
        {
            buttonImage.color = wrongColor;
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
            answerButtons[i].GetComponent<Image>().color = defaultColors[i]; // Reset button color
        }
    }

    public void SelectAnswer(int answerIndex)
    {
        if (!QuizManager.Instance.IsQuizActive()) return;

        bool isCorrect = QuizManager.Instance.CheckAnswer(answerIndex);
        Image buttonImage = answerButtons[answerIndex].GetComponent<Image>(); // Get the button image


        if (isCorrect)
        {
            buttonImage.color = correctColor;
            CorrectFX.Play();
        }
        else
        {
            buttonImage.color = wrongColor;
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

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponent<Image>().color = defaultColors[i]; // Reset to default color
            answerButtons[i].GetComponent<Button>().enabled = true;
        }

        QuizManager.Instance.DisplayNextQuestion();
    }
}