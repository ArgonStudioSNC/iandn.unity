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
    public Transform logo;
    public Transform login;
    public Button accessButton;
    public InputField codeInput;
    public VideoPlayer videoPlayer;

    protected void Awake()
    {
        videoPlayer.Prepare();
    }

    protected void Start()
    {
        login.gameObject.SetActive(false);
        StartCoroutine(StartWelcomeScreen());
    }

    public void CheckCode()
    {
        string code = codeInput.text;
        Dictionary<string, string> credentials = CredentialsHelper.GetDictionary(credentialsFile);

        if (code.Equals(credentials["code-apero"]))
        {
            PlayerPrefs.SetInt("logged", 1);
            AccessApp();
        }
        else if (code.Equals(credentials["code-souper"]))
        {
            PlayerPrefs.SetInt("souper", 1);
            PlayerPrefs.SetInt("logged", 1);
            AccessApp();
        }
        else
        {
            errorText.text = "Code non valide";
            ResetError(2);
        }
    }

    public void AccessApp()
    {
        PlayerPrefs.SetInt("first-access", 0);
        StartCoroutine(LoadYourAsyncScene("I&N"));
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
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f);
        while (!videoPlayer.isPrepared)
        {
            yield return waitForSeconds;
            break;
        }

        videoPlayer.Play();
        while (videoPlayer.frame < 1)
        {
            yield return null;
        }
        logo.GetComponentInChildren<RawImage>().texture = videoPlayer.texture;

        yield return WaitForVideoPlayer(videoPlayer);

        if (0 != PlayerPrefs.GetInt("logged", 0))
        {
            if (0 == PlayerPrefs.GetInt("first-access", 1))
            {
                AccessApp();
                yield break;
            }
            PlayerPrefs.SetInt("first-access", 1);
            codeInput.gameObject.SetActive(false);
            accessButton.gameObject.SetActive(true);
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

    private IEnumerator ResetError(int time)
    {
        yield return new WaitForSeconds(time);
        errorText.text = "";
    }
}

