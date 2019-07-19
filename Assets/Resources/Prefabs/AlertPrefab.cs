using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AlertPrefab : MonoBehaviour
{
    public static void LaunchAlert(string message)
    {
        AlertPrefab existingAlert = FindObjectOfType<AlertPrefab>();
        if (existingAlert) Destroy(existingAlert.gameObject);
        GameObject alert = Instantiate(Resources.Load("Prefabs/Alert", typeof(GameObject))) as GameObject;
        alert.GetComponent<AlertPrefab>().SetMessage(message);
    }

    public Text textComponent;
    public float fadeInDelay = 0.3f;

    private CanvasGroup m_canvasGroup;
    private float m_goalAlpha = 1f;

    protected void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_canvasGroup.alpha = 0.00001f;
    }

    protected void Start()
    {
        StartCoroutine(AlertCoroutine());
    }

    protected void Update()
    {
        if (m_canvasGroup.alpha <= 0f) Destroy(gameObject);

        if (m_canvasGroup.alpha <= m_goalAlpha)
        {
            m_canvasGroup.alpha += Time.deltaTime / fadeInDelay;
        }
        else if (m_canvasGroup.alpha >= m_goalAlpha)
        {
            m_canvasGroup.alpha -= Time.deltaTime / fadeInDelay;
        }
    }

    public void SetMessage(string message)
    {
        textComponent.text = message;
    }

    private IEnumerator AlertCoroutine()
    {
        yield return new WaitForSeconds(2.0f);
        m_goalAlpha = 0f;
    }
}