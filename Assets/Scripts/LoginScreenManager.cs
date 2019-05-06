using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoginScreenManager : MonoBehaviour
{
    public TextAsset credentialsFile;
    public Text errorText;
    public Text codeText;
    public Transform logo;
    public Transform login;
    public VideoPlayer videoPlayer;

    protected void Start()
    {
#if UNITY_IOS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
#endif
        StartCoroutine(StartWelcomeScreen());
    }

    public void CheckCode()
    {
        string code = codeText.text;
        Dictionary<string, string> credentials = CredentialsHelper.GetDictionary(credentialsFile);

        if (code.Equals(credentials["code-apero"]))
        {
            PersistentToken.SetLogged(true);
            AccessApp();
        }
        else if (code.Equals(credentials["code-souper"]))
        {
            PersistentToken.SetMealAccess(true);
            PersistentToken.SetLogged(true);
            AccessApp();
        }
        else
        {
            errorText.text = "Code non valide";
        }
    }

    public void AccessApp()
    {
        StartCoroutine(LoadYourAsyncScene("MainScene"));
    }

    private IEnumerator LoadYourAsyncScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator StartWelcomeScreen()
    {
        videoPlayer.Stop();
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }

        logo.GetComponentInChildren<RawImage>().texture = videoPlayer.texture;
        videoPlayer.Play();
        yield return WaitForVideoPlayer(videoPlayer);

        if (PersistentToken.IsLogged())
        {
            AccessApp();
            yield break;
        }

        Animation anim = logo.GetComponent<Animation>();
        anim.Play();
        yield return WaitForAnimation(anim);

        login.gameObject.SetActive(true);
        login.GetComponentInChildren<Animation>().Play();

    }

    private IEnumerator WaitForAnimation(Animation animation)
    {
        do
        {
            yield return null;
        } while (animation.isPlaying);
    }

    private IEnumerator WaitForVideoPlayer(VideoPlayer vp)
    {
        do
        {
            yield return null;
        } while (vp.isPlaying);
    }
}

