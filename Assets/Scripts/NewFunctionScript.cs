using System;
using UnityEngine;
using UnityEngine.UI;

public class NewFunctionScript : MonoBehaviour
{
    private DateTime disableTime = new DateTime(2019, 12, 13, 0, 0, 0);
    private Text m_text;

    protected void Awake()
    {
        m_text = GetComponent<Text>();
        if (disableTime.Subtract(DateTime.Now).Milliseconds > 0)
        {
            m_text.text = string.Concat(m_text.text, "<size=9><color=#87130e> (NEW!)</color></size>");
        }
    }
}
