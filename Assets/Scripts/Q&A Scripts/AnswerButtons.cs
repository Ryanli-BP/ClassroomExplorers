using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtons : MonoBehaviour
{
    // Arrays for button backgrounds
    [SerializeField] private GameObject[] answerBackBlue; // Default backgrounds
    [SerializeField] private GameObject[] answerBackGreen; // Green for correct answers
    [SerializeField] private GameObject[] answerBackRed; // Red for incorrect answers

    [SerializeField] private GameObject[] answerButtons; // The buttons themselves

    [SerializeField] private AudioSource CorrectFX, WrongFX;

    // Called when an answer is selected
    public void SelectAnswer(int answerIndex)
    {

        Debug.Log(answerIndex);
        Debug.Log(QuestionGenerate.ActualAnswer);
        // Check if the selected answer is correct
        if (QuestionGenerate.ActualAnswer == GetAnswerLabel(answerIndex))
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

        // Disable all buttons after an answer is selected
        DisableAllButtons();
        StartCoroutine(NextQuestion());
    }

    private string GetAnswerLabel(int index)
    {
        // Convert 0, 1, 2, 3 to "A", "B", "C", "D"
        return ((char)('A' + index)).ToString();
    }

    private void DisableAllButtons()
    {
        foreach (GameObject button in answerButtons)
        {
            button.GetComponent<Button>().enabled = false;
        }
    }

    IEnumerator NextQuestion()
    {
        yield return new WaitForSeconds(2.0f);

        // Reset the button backgrounds
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

        // Enable all buttons
        foreach (GameObject button in answerButtons)
        {
            button.GetComponent<Button>().enabled = true;
        }

        // Display the next question
        QuestionGenerate.displayingQuestion = false;
    }
}
