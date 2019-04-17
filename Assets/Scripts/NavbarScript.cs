using UnityEngine;
using System;
using UnityEngine.UI;

public class NavbarScript : MonoBehaviour
{
    public Transform titleText;
    public Transform backButton;

    private ScreenManager m_screenManager;
    private MainSceneManager m_mainSceneManager;
    private Text m_text;

    protected void Start()
    {
        m_mainSceneManager = FindObjectOfType<MainSceneManager>();
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_text = titleText.GetComponent<Text>();
    }

    protected void Update()
    {
        if (!m_screenManager.IsScreenOpen())
        {
            backButton.gameObject.SetActive(false);

            TimeSpan countdown = m_mainSceneManager.weddingDate.Subtract(DateTime.Now);
            m_text.text = stringBuilder(countdown);
        }
        else
        {
            backButton.gameObject.SetActive(true);
        }
    }

    public void SetText(string newtext)
    {
        m_text.text = newtext;
    }

    private string stringBuilder(TimeSpan duration)
    {
        string value = "";
        value += duration.Days + " j : ";
        value += duration.Hours + " h : ";
        value += duration.Minutes + " m : ";
        value += duration.Seconds + " s";
        return value;
    }
}
