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
            QuestionDisplay.newQuestion = "Which one is a strawberry";
            QuestionDisplay.newA = "A. [image of orange]";
            QuestionDisplay.newB = "B. [image of apple]";
            QuestionDisplay.newC = "C. [image of banna]";
            QuestionDisplay.newD = "D. [image of strawberry]";
            ActualAnswer = "D";
        }
    }
}
