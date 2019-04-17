using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CursorScript : MonoBehaviour
{
    [Range(0.0f, 24.00f)]
    public float[] decimalCheckpoints;

    private float m_startupDuration = 7.5f;
    private bool m_isStarting = false;
    private float time;
    private RectTransform m_rectTransform;
    private MainSceneManager m_mainSceneManager;
    private DateTime[] m_checkpoints;
    private TimeSpan m_partyDuration;
    private DateTime m_now;
    private Image m_image;

    protected void Start()
    {
        m_checkpoints = GenerateCheckpoints(decimalCheckpoints);
        m_partyDuration = m_checkpoints[m_checkpoints.Length - 1].Subtract(m_checkpoints[0]);
    }

    protected void Awake()
    {
        m_image = transform.gameObject.GetComponent<Image>();
        m_rectTransform = transform.GetComponent<RectTransform>();
        m_mainSceneManager = FindObjectOfType<MainSceneManager>();
    }

    protected void OnEnable()
    {
        time = 0.0f;
        m_image.enabled = true;
        m_isStarting = true;
        m_rectTransform.anchorMin = new Vector2(0.5f, 1.1f);
        m_rectTransform.anchorMax = new Vector2(0.5f, 1.1f);
    }

    protected void Update()
    {
        m_now = DateTime.Now;
        if (m_isStarting)
        {
            time += Time.deltaTime;
            if (m_now < m_checkpoints[0])
            {
                float anchor = ClampAnchor(1.0f - (time / m_startupDuration));

                m_rectTransform.anchorMin = new Vector2(0.5f, anchor);
                m_rectTransform.anchorMax = new Vector2(0.5f, anchor);

                if (time >= m_startupDuration) StartCoroutine(ResetAfter(1.5f));
            }
            else
            {
                float goalAnchor = GetAnchorPosition();
                float distance = 1.0f - goalAnchor;
                float dur = 1.5f;

                float anchor = ClampAnchor(1.0f - (time / dur) * distance);

                m_rectTransform.anchorMin = new Vector2(0.5f, anchor);
                m_rectTransform.anchorMax = new Vector2(0.5f, anchor);

                if (time >= dur) m_isStarting = false;
            }
        }
        else
        {
            m_image.enabled = m_now >= m_checkpoints[0];
            float anchor = GetAnchorPosition();
            Debug.Log(anchor);
            m_rectTransform.anchorMin = new Vector2(0.5f, anchor);
            m_rectTransform.anchorMax = new Vector2(0.5f, anchor);
        }
    }

    private IEnumerator ResetAfter(float time)
    {
        yield return new WaitForSeconds(time);
        m_isStarting = false;
    }

    private DateTime[] GenerateCheckpoints(float[] cp)
    {
        DateTime wedding = m_mainSceneManager.weddingDate;
        DateTime[] result = new DateTime[cp.Length];

        for (int i = 0; i < cp.Length; i++)
        {
            int hours = Convert.ToInt32(Math.Floor(cp[i]));
            int minutes = Convert.ToInt32((cp[i] - hours) * 60.0f);

            DateTime dt = new DateTime(wedding.Year, wedding.Month, wedding.Day, hours, minutes, 0);
            result[i] = dt;
        }

        return result;
    }

    private float GetAnchorPosition()
    {
        if (m_now <= m_checkpoints[0]) return 1.01f;
        if (m_now >= m_checkpoints[m_checkpoints.Length - 1]) return 0.00f;

        float fixedStep = 1.0f / (m_checkpoints.Length - 1);
        float anchor = 1.0f;

        for (int i = 0; i < m_checkpoints.Length - 1; i++)
        {
            if (m_now > m_checkpoints[i + 1])
            {
                anchor -= fixedStep;
            }
            else
            {
                double percent = m_now.Subtract(m_checkpoints[i]).TotalSeconds / m_checkpoints[i + 1].Subtract(m_checkpoints[i]).TotalSeconds;
                anchor -= (float)percent * fixedStep;
                return anchor;
            }
        }
        return anchor;
    }

    private float ClampAnchor(float anchor)
    {
        return Math.Min(Math.Max(anchor, 0.0f), 1.0f);
    }
}
