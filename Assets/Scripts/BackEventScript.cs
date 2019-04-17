using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackEventScript : MonoBehaviour
{
    private AndroidJavaObject m_activity;

    protected void Start()
    {
        m_activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
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
#if UNITY_ANDROID
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            m_activity.Call<bool>("moveTaskToBack", true);
            return;
        }
#endif
        ScreenManager screenManager = FindObjectOfType<ScreenManager>();
        if (screenManager == null) return;

        if (!PersistentToken.IsLogged())
        {
            StartCoroutine(LoadYourAsyncScene("LoginScreen"));
            return;
        }

        if (screenManager.IsScreenOpen())
        {
            screenManager.CloseCurrent();
            return;
        }
#if UNITY_ANDROID
        m_activity.Call<bool>("moveTaskToBack", true);
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
