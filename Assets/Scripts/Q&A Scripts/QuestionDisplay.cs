using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using System.Collections;

public class QuestionDisplay : MonoBehaviour
{

    [SerializeField] private GameObject ScreenQuestion;
    [SerializeField] private GameObject AnswerA;
    [SerializeField] private GameObject AnswerB;
    [SerializeField] private GameObject AnswerC;
    [SerializeField] private GameObject AnswerD;
    [SerializeField] private static string newQuestion;
    [SerializeField] private static string newA;
    [SerializeField] private static string newB;
    [SerializeField] private static string newC;
    [SerializeField] private static string newD;

    [SerializeField] private static bool updateNow = false;

    public static string NewQuestion
    {
        get { return newQuestion; }
        set { newQuestion = value; }
    }

    public static string NewA
    {
        get { return newA; }
        set { newA = value; }
    }

    public static string NewB
    {
        get { return newB; }
        set { newB = value; }
    }

    public static string NewC
    {
        get { return newC; }
        set { newC = value; }
    }

    public static string NewD
    {
        get { return newD; }
        set { newD = value; }
    }
    public static bool UpdateNow
    {
        get { return updateNow; }
        set { updateNow = value; }
    }

    // Update is called once per frame
    void Update()
    {
        if (updateNow == false)
        {
            updateNow = true;
            StartCoroutine(PushTextOnScreen());
        }
    }

    IEnumerator PushTextOnScreen()
    {
        yield return new WaitForSeconds(0.25f);
        ScreenQuestion.GetComponent<TextMeshProUGUI>().text = newQuestion;
        AnswerA.GetComponent<TextMeshProUGUI>().text = newA;
        AnswerB.GetComponent<TextMeshProUGUI>().text = newB;
        AnswerC.GetComponent<TextMeshProUGUI>().text = newC;
        AnswerD.GetComponent<TextMeshProUGUI>().text = newD;
    }
}
