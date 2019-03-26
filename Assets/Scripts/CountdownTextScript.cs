using UnityEngine;
using System;
using UnityEngine.UI;

public class CountdownTextScript : MonoBehaviour
{
    private DateTime mWeddingDate;
    private Text mCountdownText;

    // Start is called before the first frame update
    protected void Start()
    {
        mWeddingDate = new DateTime(2019, 10, 11, 16, 0, 0);
        mCountdownText = GetComponent<Text>();
    }

    // Update is called once per frame
    protected void Update()
    {
        TimeSpan countdown = mWeddingDate.Subtract(DateTime.Now);
        mCountdownText.text = stringBuilder(countdown);
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
