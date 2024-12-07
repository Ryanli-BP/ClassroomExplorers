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
            QuestionDisplay.newQuestion = "What's 1 + 2";
            QuestionDisplay.newA = "A. 1";
            QuestionDisplay.newB = "B. 2";
            QuestionDisplay.newC = "C. 3";
            QuestionDisplay.newD = "D. 4";
            ActualAnswer = "C";
        }
    }
}
