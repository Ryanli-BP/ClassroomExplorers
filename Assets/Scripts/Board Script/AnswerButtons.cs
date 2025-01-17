using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtons : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private GameObject[] answerButtons;
    [SerializeField] private GameObject[] answerBackBlue;
    [SerializeField] private GameObject[] answerBackGreen;
    [SerializeField] private GameObject[] answerBackRed;

    [Header("Audio")]
    [SerializeField] private AudioSource CorrectFX;
    [SerializeField] private AudioSource WrongFX;

    private int selectedAnswerIndex = -1;
    private bool lastAnswerCorrect = false;

        private void Start()
    {
        Camera arCamera = GameObject.Find("AR Camera").GetComponent<Camera>();
        if (Camera.main != null && !Camera.main.GetComponent<AudioListener>())
        {
            Debug.LogWarning("Adding AudioListener to Main Camera");
            Camera.main.gameObject.AddComponent<AudioListener>();
        }
        
        // Setup button listeners
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int buttonIndex = i;
            Button button = answerButtons[i].GetComponent<Button>();
            button.onClick.AddListener(() => OnAnswerSelected(buttonIndex));
        }
    }

    public void OnAnswerSelected(int answerIndex)
    {
        Debug.Log($"Button clicked: {answerIndex}"); // Debug which button was clicked
        
        selectedAnswerIndex = answerIndex;
        lastAnswerCorrect = QuizManager.Instance.CheckAnswer(answerIndex);

        // Reset all panels first
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerBackBlue[i].SetActive(true);
            answerBackGreen[i].SetActive(false);
            answerBackRed[i].SetActive(false);
        }

        // Update only the clicked button's panels
        answerBackBlue[selectedAnswerIndex].SetActive(false);
        
        if (lastAnswerCorrect)
        {
            answerBackGreen[selectedAnswerIndex].SetActive(true);
            CorrectFX.Play();
        }
        else
        {
            answerBackRed[selectedAnswerIndex].SetActive(true);
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

    public void EnableButtons()
    {
        foreach (GameObject button in answerButtons)
        {
            button.GetComponent<Button>().enabled = true;
        }

        // Reset all panels
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerBackBlue[i].SetActive(true);
            answerBackGreen[i].SetActive(false);
            answerBackRed[i].SetActive(false);
        }
    }

    private IEnumerator NextQuestion()
    {
        yield return new WaitForSeconds(1f);
        EnableButtons(); // Reset panels before next question
        QuizManager.Instance.AnswerQuestion(lastAnswerCorrect);
    }
}