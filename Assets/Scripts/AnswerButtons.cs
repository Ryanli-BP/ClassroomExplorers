using UnityEngine;
using UnityEngine.UI;

public class AnswerButtons : MonoBehaviour
{
    public GameObject answerAbackBlue, answerAbackGreen, answerAbackRed; //Blue is default, Green is Correct, Red is wrong
    public GameObject answerBbackBlue, answerBbackGreen, answerBbackRed;
    public GameObject answerCbackBlue, answerCbackGreen, answerCbackRed;
    public GameObject answerDbackBlue, answerDbackGreen, answerDbackRed;

    public GameObject answerA, answerB, answerC, answerD;

    public AudioSource CorrectFX, WrongFX;

    public void AnswerA()
    {
        if (QuestionGenerate.ActualAnswer == "A")
        {
            answerAbackGreen.SetActive(true);
            answerAbackBlue.SetActive(false);
            CorrectFX.Play();
        }
        else
        {
            answerAbackRed.SetActive(true);
            answerAbackBlue.SetActive(false);
            WrongFX.Play();
        }
        answerA.GetComponent<Button>().enabled = false;
        answerB.GetComponent<Button>().enabled = false;
        answerC.GetComponent<Button>().enabled = false;
        answerD.GetComponent<Button>().enabled = false;
    }

    public void AnswerB()
    {
        if (QuestionGenerate.ActualAnswer == "B")
        {
            answerBbackGreen.SetActive(true);
            answerBbackBlue.SetActive(false);
            CorrectFX.Play();
        }
        else
        {
            answerBbackRed.SetActive(true);
            answerBbackBlue.SetActive(false);
            WrongFX.Play();
        }
        answerA.GetComponent<Button>().enabled = false;
        answerB.GetComponent<Button>().enabled = false;
        answerC.GetComponent<Button>().enabled = false;
        answerD.GetComponent<Button>().enabled = false;
    }

    public void AnswerC()
    {
        if (QuestionGenerate.ActualAnswer == "C")
        {
            answerCbackGreen.SetActive(true);
            answerCbackBlue.SetActive(false);
            CorrectFX.Play();
        }
        else
        {
            answerCbackRed.SetActive(true);
            answerCbackBlue.SetActive(false);
            WrongFX.Play();
        }
        answerA.GetComponent<Button>().enabled = false;
        answerB.GetComponent<Button>().enabled = false;
        answerC.GetComponent<Button>().enabled = false;
        answerD.GetComponent<Button>().enabled = false;
    }

    public void AnswerD()
    {
        if (QuestionGenerate.ActualAnswer == "D")
        {
            answerDbackGreen.SetActive(true);
            answerDbackBlue.SetActive(false);
            CorrectFX.Play();
        }
        else
        {
            answerDbackRed.SetActive(true);
            answerDbackBlue.SetActive(false);
            WrongFX.Play();
        }
        answerA.GetComponent<Button>().enabled = false;
        answerB.GetComponent<Button>().enabled = false;
        answerC.GetComponent<Button>().enabled = false;
        answerD.GetComponent<Button>().enabled = false;
    }
}
