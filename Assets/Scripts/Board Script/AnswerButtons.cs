using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtons : MonoBehaviour
{
    [SerializeField] private GameObject[] answerBackBlue;
    [SerializeField] private GameObject[] answerBackGreen;
    [SerializeField] private GameObject[] answerBackRed;
    [SerializeField] private GameObject[] answerButtons;
    [SerializeField] private AudioSource CorrectFX, WrongFX;

    public void SelectAnswer(int answerIndex)
    {
        if (!QuizManager.Instance.IsQuizActive()) return;

        bool isCorrect = QuizManager.Instance.CheckAnswer(answerIndex);

        if (isCorrect)
        {
            answerBackGreen[answerIndex].SetActive(true);
            answerBackBlue[answerIndex].SetActive(false);
            CorrectFX.Play();
        }
        else
        {
            answerBackRed[answerIndex].SetActive(true);
            answerBackBlue[answerIndex].SetActive(false);
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

        foreach (GameObject button in answerBackBlue)
        {
            button.SetActive(true);
        }

        foreach (GameObject button in answerBackGreen)
        {
            button.SetActive(false);
        }

        foreach (GameObject button in answerBackRed)
        {
            button.SetActive(false);
        }

        foreach (GameObject button in answerButtons)
        {
            button.GetComponent<Button>().enabled = true;
        }

        QuizManager.Instance.DisplayNextQuestion();
    }
}