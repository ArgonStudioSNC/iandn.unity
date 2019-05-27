using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WeddingsMenuScript : MonoBehaviour
{
    public bool resetPlayerPrefsAtStartup = false;
    public Transform codeLogic;
    public Transform confirmationLogic;
    public Text placeholderText;
    public TextAsset credentialsFile;

    private InputField m_codeInputField;

    protected void Awake()
    {
        if (resetPlayerPrefsAtStartup) PlayerPrefs.DeleteAll();

#if UNITY_IOS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
#endif

        //bypass on Android
#if UNITY_ANDROID && !UNITY_EDITOR
        SceneManager.LoadScene("I&N-Loader");
#else
        if (PlayerPrefs.HasKey("quick-access-code") && 0 != PlayerPrefs.GetInt("logged", 0))
        {
            CodeCheckInner(PlayerPrefs.GetString("quick-access-code"));
        }
#endif
    }

    protected void Start()
    {
        m_codeInputField = codeLogic.GetComponentInChildren<InputField>();
        m_codeInputField.text = PlayerPrefs.GetString("quick-access-code", string.Empty);
    }


    public void CodeCheck()
    {
        CodeCheckInner(m_codeInputField.text);
    }

    public void ResetAll()
    {
        placeholderText.text = "Saisir le code";
        m_codeInputField.text = "";
        codeLogic.gameObject.SetActive(true);
        confirmationLogic.gameObject.SetActive(false);
    }

    public void RememberCode(bool value)
    {
        PlayerPrefs.SetString("quick-access-code", value ? m_codeInputField.text : string.Empty);
        AccessApp();
    }

    private void CodeCheckInner(string code)
    {
        Dictionary<string, string> credentials = CredentialsHelper.GetDictionary(credentialsFile);

        if (code.Equals(credentials["code-apero"]))
        {
            PlayerPrefs.SetInt("logged", 1);
            codeLogic.gameObject.SetActive(false);
            confirmationLogic.gameObject.SetActive(true);
        }
        else if (code.Equals(credentials["code-souper"]))
        {
            PlayerPrefs.SetInt("souper", 1);
            PlayerPrefs.SetInt("logged", 1);
            codeLogic.gameObject.SetActive(false);
            confirmationLogic.gameObject.SetActive(true);
        }
        else
        {
            placeholderText.text = "<color=\"red\">Code non valide</color>";
            m_codeInputField.text = "";
            StartCoroutine(ResetError(1.2f));
        }
    }

    private void AccessApp()
    {
        StartCoroutine(LoadYourAsyncScene("I&N-Loader"));
    }

    private IEnumerator LoadYourAsyncScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private IEnumerator ResetError(float time)
    {
        yield return new WaitForSeconds(time);
        ResetAll();
    }
}
