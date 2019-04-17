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
    public Texture lastFrame;

    protected void Start()
    {
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
        videoPlayer.Prepare();
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }
        logo.GetComponent<RawImage>().texture = videoPlayer.texture;
        videoPlayer.Play();
        yield return WaitForVideoPlayer(videoPlayer);
        logo.GetComponent<RawImage>().texture = lastFrame;

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

