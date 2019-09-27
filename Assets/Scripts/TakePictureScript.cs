using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TakePictureScript : MonoBehaviour
{
    public Animator animator;
    public Image popup;

    private string m_key = "hasPosted";
    private Color m_color;

    protected void Awake()
    {
        if (!PlayerPrefs.HasKey(m_key)) PlayerPrefs.SetInt(m_key, 0);
        m_color = popup.color;
    }

    protected void OnEnable()
    {
        m_color.a = 0f;
        popup.color = m_color;
        if (PlayerPrefs.GetInt(m_key) == 0) StartCoroutine(Tutorial());
    }

    protected void OnDisable()
    {
        animator.SetBool("tutorial", false);
        animator.transform.GetComponent<RectTransform>().localEulerAngles = new Vector3(0, 0, 0);
    }

    private IEnumerator Tutorial()
    {
        yield return new WaitForSeconds(0.8f);

        m_color.a = 1f;
        popup.color = m_color;
        animator.SetBool("tutorial", true);

        yield return new WaitForSeconds(2.5f);


        float targetAlpha = 0.0f;
        float time = 1.2f;
        while (m_color.a > targetAlpha)
        {
            m_color.a -= Time.deltaTime / time;
            popup.color = m_color;
            yield return null;
        }

        animator.SetBool("tutorial", false);
    }
}
