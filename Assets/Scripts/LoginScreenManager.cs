using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScreenManager : MonoBehaviour
{
    public TextAsset credentialsFile;
    public Text errorText;
    public Text codeText;

    protected void Start()
    {
        if (PersistentToken.IsLogged())
        {
            AccessApp();
        }
        else
        {

        }
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
        } else
        {
            errorText.text = "Code non valide";
        }
    }

    private void AccessApp()
    {
        StartCoroutine(LoadYourAsyncScene());
    }
    
    private IEnumerator LoadYourAsyncScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("MainScene");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}

