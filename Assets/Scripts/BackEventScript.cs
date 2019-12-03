using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackEventScript : MonoBehaviour
{
    public Transform popup;

    private ScreenManager m_screenManager;

    protected void Start()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
    }

    protected void Update()
    {
#if UNITY_ANDROID
        // On Android, the Back button is mapped to the Esc key
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GoBack();
        }
#endif
    }

    public void GoBack()
    {
        if (m_screenManager == null)
        {
#if UNITY_IOS || UNITY_EDITOR
            popup.gameObject.SetActive(true);
#else
            return;
#endif
        }
        else
        {
            if (m_screenManager.IsScreenOpen())
            {
                m_screenManager.CloseCurrent();
                return;
            }
            else
            {
#if UNITY_IOS || UNITY_EDITOR
                popup.gameObject.SetActive(true);
#else           
                return;
#endif
            }
        }
    }

    public void GoBackHard()
    {
#if UNITY_IOS || UNITY_EDITOR
        PlayerPrefs.DeleteKey("logged");
        SceneManager.LoadSceneAsync(0);
#endif
    }

    private IEnumerator LoadYourAsyncScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
