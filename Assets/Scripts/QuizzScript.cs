﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QuizzScript : MonoBehaviour
{
    private static readonly string[] RESULT_TEXTS = new string[] {
        "Ah voilà… Vous être sûr que vous êtes au bon endroit, au mariage de Nicolas et Delphine?!",
        "Nathan (qui, on vous le dit, est architecte) ne vous fera pas un dessin, ce n’est pas brillant...",
        "Aïe, vous ne connaissez pas bien les mariés. En même temps, ce n’est pas de votre faute s’ils ne vous invitent jamais à manger chez eux...",
        "Pas si mal. Même si vous avez sûrement eu votre dose de chance sur certaines questions, les mariés seraient fiers de vous.",
        "Joli! Soit vous connaissez parfaitement nos mariés, soit vous avez (très bien) répondu au hasard. Il y a des tribolos au kiosque du coin, si jamais.",
        "Isabel, Nathan! Il vous était interdit de répondre vous-mêmes au quiz!"
    };

    public enum LoadingState
    {
        Downloading,
        Success,
        Error
    }


    [Serializable]
    internal class Question
    {
        public uint id;
        public int question_nb;
        public string question;
        public int correct_answer_id;
        public string answer_A;
        public string answer_B;
        public string answer_C;
        public string answer_D;

        public Question(uint id, int question_nb, string question, int correct_answer_id, string answer_A, string answer_B, string answer_C, string answer_D)
        {
            this.id = id;
            this.question_nb = question_nb;
            this.question = question;
            this.correct_answer_id = correct_answer_id;
            this.answer_A = answer_A;
            this.answer_B = answer_B;
            this.answer_C = answer_C;
            this.answer_D = answer_D;
        }
    }


    [Serializable]
    internal class Quizz
    {
        public List<Question> questions;

        public Quizz(List<Question> questions)
        {
            this.questions = questions;
        }
    }


    public Transform questionTransform;
    public Transform endOfQuizzPanel;

    public int QuizzScore
    {
        get
        {
            if (!PlayerPrefs.HasKey("quizz_score")) PlayerPrefs.SetInt("quizz_score", 0);
            return PlayerPrefs.GetInt("quizz_score");
        }
        set { PlayerPrefs.SetInt("quizz_score", value); }
    }


    public int QuizzState
    {
        get
        {
            if (!PlayerPrefs.HasKey("quizz_state")) PlayerPrefs.SetInt("quizz_state", 0);
            return PlayerPrefs.GetInt("quizz_state");
        }
        set { PlayerPrefs.SetInt("quizz_state", value); }
    }


    private ScreenManager m_screenManager;
    private QuestionScript m_questionScript;
    private Animator m_quizzQuestionAnimator;

    private Quizz m_quizz;
    private LoadingState m_currentState = LoadingState.Error;

    private UnityWebRequest www { get; set; }


    protected void Awake()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_questionScript = questionTransform.GetComponent<QuestionScript>();
        m_quizzQuestionAnimator = questionTransform.GetComponent<Animator>();
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
        m_currentState = LoadingState.Error;
        EnableEndOfQuizzPanel(false);
    }


    public void PlayQuizz()
    {
        if (m_currentState != LoadingState.Downloading)
        {
            m_currentState = LoadingState.Downloading;
            StartCoroutine(DownloadQuizzCoroutine());
        }
    }


    public void RestartQuizz()
    {
        QuizzState = 0;
        QuizzScore = 0;
        EnableEndOfQuizzPanel(false);
        PlayQuizz();
    }


    public void AnswerQuestion(bool isCorrect)
    {
        if (isCorrect)
        {
            QuizzScore += 1;
            AlertPrefab.LaunchAlert("Bonne réponse !");
        }
        QuizzState += 1;

        if (QuizzState >= m_quizz.questions.Count) QuizzState = -1;
    }


    public void NextQuestion()
    {
        if (QuizzState == -1)
        {
            AlertPrefab.LaunchAlert("Vous avez répondu à toutes les questions du quizz");
            if (m_questionScript.isActiveAndEnabled) m_screenManager.CloseCurrent();
            EnableEndOfQuizzPanel(true);
        }
        else
        {
            if (!m_questionScript.isActiveAndEnabled) m_screenManager.OpenPanel(m_quizzQuestionAnimator);

            Question question = m_quizz.questions[QuizzState];
            List<string> answers = new List<string>();
            answers.Add(question.answer_A);
            answers.Add(question.answer_B);
            answers.Add(question.answer_C);
            answers.Add(question.answer_D);
            m_questionScript.SetQuestion(question.question_nb, m_quizz.questions.Count, question.question, answers, question.correct_answer_id);
        }
    }


    private void EnableEndOfQuizzPanel(bool val)
    {
        if (val)
        {
            int score = QuizzScore;
            string endofquizzText = "Vous avez donné " + score + "  bonnes réponses sur  " + m_quizz.questions.Count + " questions\n\n";
            if (score <= 2) endofquizzText = string.Concat(endofquizzText, RESULT_TEXTS[0]);
            else if (score <= 5) endofquizzText = string.Concat(endofquizzText, RESULT_TEXTS[1]);
            else if (score <= 8) endofquizzText = string.Concat(endofquizzText, RESULT_TEXTS[2]);
            else if (score <= 11) endofquizzText = string.Concat(endofquizzText, RESULT_TEXTS[3]);
            else if (score <= 14) endofquizzText = string.Concat(endofquizzText, RESULT_TEXTS[4]);
            else if (score > 14) endofquizzText = string.Concat(endofquizzText, RESULT_TEXTS[5]);
            endOfQuizzPanel.GetComponentInChildren<Text>().text = endofquizzText;
        }

        endOfQuizzPanel.gameObject.SetActive(val);
    }

    private IEnumerator DownloadQuizzCoroutine()
    {
        Uri uri = new Uri("https://www.iandn.app/quizz/d/");

        using (www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                if (www.responseCode == 403) AlertPrefab.LaunchAlert("Patience. Le quizz sera accessible dès l'apéro!");
                else AlertPrefab.LaunchAlert("Échec du chargement. Êtes-vous connecté à internet?");

                m_currentState = LoadingState.Error;
                yield return null;
            }
            else
            {
                m_quizz = JsonUtility.FromJson<Quizz>("{\"questions\":" + www.downloadHandler.text + "}");
                m_currentState = LoadingState.Success;
                NextQuestion();
            }
        }
    }

}