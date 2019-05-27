using System;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneManager : MonoBehaviour
{
    public DateTime weddingDate { get; private set; } = new DateTime(2019, 10, 11, 15, 10, 0);

    public Transform navbar;
    public Transform mainMenu;

    public Transform[] disabledWithoutMealAccess;

    private ScreenManager m_screenManager;
    private Transform m_homeScreen;
    private Transform m_backButton;
    private Text m_text;

    protected void Start()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_homeScreen = mainMenu.parent;
        m_text = navbar.Find("TitleText").GetComponent<Text>();
        m_backButton = navbar.Find("BackButton");

        if (0 == PlayerPrefs.GetInt("souper", 0))
        {
            foreach (Transform t in disabledWithoutMealAccess)
            {
                t.gameObject.SetActive(false);
            }
        }
    }

    protected void Update()
    {
        if (!m_screenManager.IsScreenOpen())
        {
            TimeSpan countdown = weddingDate.Subtract(DateTime.Now);
            m_text.text = countdown.Milliseconds >= 0 ? stringBuilder(countdown) : "Just Married";
        }

#if UNITY_ANDROID && ! UNITY_EDITOR
        if (!m_screenManager.IsScreenOpen())
        {
            m_backButton.gameObject.SetActive(false);
        }
        else
        {
            m_backButton.gameObject.SetActive(true);
        }
#endif
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
