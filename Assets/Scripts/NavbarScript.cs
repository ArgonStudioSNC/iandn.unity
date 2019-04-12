using UnityEngine;
using System;
using UnityEngine.UI;

public class NavbarScript : MonoBehaviour
{
    public Transform titleText;
    public Transform backButton;

    private DateTime mWeddingDate;
    private ScreenManager mScreenManager;
    private Text mText;

    // Start is called before the first frame update
    protected void Start()
    {
        mScreenManager = FindObjectOfType<ScreenManager>();
        mWeddingDate = new DateTime(2019, 10, 11, 16, 0, 0);
        mText = titleText.GetComponent<Text>();
    }

    // Update is called once per frame
    protected void Update()
    {
        if (!mScreenManager.IsScreenOpen())
        {
            backButton.gameObject.SetActive(false);

            TimeSpan countdown = mWeddingDate.Subtract(DateTime.Now);
            mText.text = stringBuilder(countdown);
        } else
        {
            backButton.gameObject.SetActive(true);
        }
    }

    public void SetText(string newtext)
    {
        mText.text = newtext;
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
