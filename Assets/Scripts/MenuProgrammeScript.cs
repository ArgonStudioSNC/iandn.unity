using UnityEngine;

public class MenuProgrammeScript : MonoBehaviour
{
    public Animator aperoPanel;
    public Animator souperPanel;

    private ScreenManager m_screneManager;

    protected void Start()
    {
        m_screneManager = FindObjectOfType<ScreenManager>();
    }

    public void OpenProgrammePage()
    {
        m_screneManager.OpenPanel((0 != PlayerPrefs.GetInt("souper")) ? souperPanel : aperoPanel);
    }
}
