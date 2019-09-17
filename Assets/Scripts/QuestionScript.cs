using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestionScript : MonoBehaviour
{
    public Text questionNumberText;
    public Transform questionTransform;
    public Transform[] answersTransform;
    public Transform nextQuestionTransform;
    public Animator animator;


    private bool m_canAnswer;
    private int m_rightAnswerID;
    private QuizzScript m_quizz;

    private Color m_grey = new Color32(0, 0, 0, 100);
    private Color m_green = new Color32(75, 93, 64, 255);
    private Color m_red = new Color32(135, 19, 13, 220);


    protected void Awake()
    {
        m_quizz = FindObjectOfType<QuizzScript>();
    }

    protected void OnDisable()
    {
        questionNumberText.text = "Quizz";
    }


    public void SetQuestion(int questionNb, int questionCount, string question, List<string> answers, int correctAnswerID)
    {
        ResetQuestion();
        questionNumberText.text = "Question " + (questionNb + 1) + " sur " + questionCount;
        questionTransform.GetComponent<Text>().text = question;
        List<string> shuffledAnswers = new List<string>(answers);
        shuffledAnswers.Shuffle();

        m_rightAnswerID = shuffledAnswers.IndexOf(answers[correctAnswerID]);

        for (int i = 0; i < answers.Count; i++)
        {
            answersTransform[i].GetComponentInChildren<Text>().text = shuffledAnswers[i];
        }

        nextQuestionTransform.gameObject.SetActive(false);

        animator.SetBool("canAnswer", true);
        m_canAnswer = true;
    }

    public void GiveAnswer(int answerID)
    {
        if (!m_canAnswer) return;
        m_canAnswer = false;
        animator.SetBool("canAnswer", false);

        m_quizz.AnswerQuestion(answerID == m_rightAnswerID);

        answersTransform[answerID].GetComponent<Image>().color = m_red;
        answersTransform[m_rightAnswerID].GetComponent<Image>().color = m_green;
        nextQuestionTransform.gameObject.SetActive(true);
    }

    private void ResetQuestion()
    {
        foreach (Transform answer in answersTransform)
        {
            answer.GetComponent<Image>().color = m_grey;
        }
    }
}


public static class IListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
}