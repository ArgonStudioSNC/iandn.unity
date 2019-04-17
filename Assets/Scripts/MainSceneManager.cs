using System;
using UnityEngine;
using UnityEngine.UI;

public class MainSceneManager : MonoBehaviour
{
    public DateTime weddingDate { get; private set; } = new DateTime(2019, 10, 11, 15, 20, 0);

    public Transform homeScreen;
    public Text titleText;
    public Transform displayedWithoutRegistration;
    public Transform[] disabledWithoutMealAccess;

    private ScreenManager m_screenManager;

    protected void Start()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();

        if (!PersistentToken.IsLogged())
        {
            homeScreen.gameObject.SetActive(false);
            titleText.text = "Isabel & Nathan";
            m_screenManager.OpenPanel(displayedWithoutRegistration.gameObject.GetComponent<Animator>());
        }

        if (!PersistentToken.hasAccessToMeal())
        {
            foreach (Transform t in disabledWithoutMealAccess)
            {
                t.gameObject.SetActive(false);
            }
        }
    }
}
