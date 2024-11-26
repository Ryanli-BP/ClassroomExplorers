using JetBrains.Annotations;
using UnityEngine;
using TMPro;
using System.Collections;

public class QuestionDisplay : MonoBehaviour
{

    public GameObject ScreenQuestion;
    public GameObject AnswerA;
    public GameObject AnswerB;
    public GameObject AnswerC;
    public GameObject AnswerD;
    public static string newQuestion;
    public static string newA;
    public static string newB;
    public static string newC;
    public static string newD;

    void Start()
    {
        StartCoroutine(PushTextOnScreen());
    }

    // Update is called once per frame
    void Update()
    {
        
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
