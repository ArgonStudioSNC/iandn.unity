using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackEventScript : MonoBehaviour
{
    private AndroidJavaObject m_activity;
    private ScreenManager m_screenManager;

    protected void Start()
    {
        m_activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
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
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (m_screenManager == null)
        {
#if UNITY_IOS
            PlayerPrefs.DeleteKey("logged");
            SceneManager.LoadSceneAsync(sceneIndex - 1);
#else
            m_activity.Call<bool>("moveTaskToBack", true);
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
#if UNITY_IOS
                PlayerPrefs.DeleteKey("logged");
                SceneManager.LoadSceneAsync(0);
#else
                m_activity.Call<bool>("moveTaskToBack", true);
                return;
#endif
            }
        }
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
