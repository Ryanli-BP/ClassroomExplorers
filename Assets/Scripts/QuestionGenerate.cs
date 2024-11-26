using TMPro;
using UnityEngine;

public class QuestionGenerate : MonoBehaviour
{
    public static string ActualAnswer;
    public static bool displayingQuestion = false;


    // Update is called once per frame
    void Update()
    {
        if (displayingQuestion == false )
        {
            displayingQuestion = true;
            QuestionDisplay.newQuestion = "Dean Mohammedally?";
            QuestionDisplay.newA = "A. Dean Mohammedally";
            QuestionDisplay.newB = "B. Mohammedallyy";
            QuestionDisplay.newC = "C. Dean";
            QuestionDisplay.newD = "D. Dean Mo";
            ActualAnswer = "A";
        }
    }
}
