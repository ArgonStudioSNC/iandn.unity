using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AddPictureScript : MonoBehaviour
{
    public RawImage picture;
    public Transform form;
    public Transform addPictureTransform;
    public Button postButton;

    private string m_picturePath = null;
    private InputField m_username;
    private InputField m_comment;

    protected void Awake()
    {
        m_username = form.FindObjectsWithTag("UsernameField").LastOrDefault().GetComponent<InputField>();
        m_comment = form.FindObjectsWithTag("CommentField").LastOrDefault().GetComponent<InputField>();
    }

    protected void OnDisable()
    {
        ResetForm();
    }

    public void GetPictureFromCamera()
    {
        if (NativeCamera.IsCameraBusy())
        {
            AlertPrefab.LaunchAlert("L'appareil photo est déjà en cours d'utilisation.");
            return;
        }
        if (NativeCamera.CheckPermission() == NativeCamera.Permission.Granted)
        {
            NativeCamera.TakePicture(GetPictureCallback);
        }
        else if (NativeCamera.CheckPermission() == NativeCamera.Permission.ShouldAsk &&
                NativeCamera.RequestPermission() == NativeCamera.Permission.Granted)
        {
            NativeCamera.TakePicture(GetPictureCallback);
        }
        else
        {
            AlertPrefab.LaunchAlert("L'application n'est pas autorisée à acceder à l'appareil photo.");
        }
    }

    public void GetPictureFromMemory()
    {
        if (NativeGallery.IsMediaPickerBusy())
        {
            AlertPrefab.LaunchAlert("MediaPicker est déjà en cours d'utilisation.");
            return;
        }
        if (NativeGallery.CheckPermission() == NativeGallery.Permission.Granted)
        {
            NativeGallery.GetImageFromGallery(GetPictureCallback, "Sélectionnez une image");
        }
        else if (NativeGallery.CheckPermission() == NativeGallery.Permission.ShouldAsk &&
                NativeGallery.RequestPermission() == NativeGallery.Permission.Granted)
        {
            NativeGallery.GetImageFromGallery(GetPictureCallback, "Sélectionnez une image");
        }
        else
        {
            AlertPrefab.LaunchAlert("L'application n'est pas autorisée à acceder à la mémoire externe");
        }
    }

    public void Publish()
    {
        postButton.interactable = false;
        if (!IsFormValide())
        {
            postButton.interactable = true;
            return;
        }
        AlertPrefab.LaunchAlert("Publication en cours...");
        string username = m_username.text;
        string comment = m_comment.text;

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        formData.Add(new MultipartFormDataSection("username", username));
        if (comment != "") formData.Add(new MultipartFormDataSection("comment", comment));
        formData.Add(new MultipartFormFileSection("picture", File.ReadAllBytes(m_picturePath), "", NativeCamera.GetImageProperties(m_picturePath).mimeType));

        StartCoroutine(PublishCoroutine(new Uri("https://iandn.app/instagram/add"), formData, (callback, message) =>
        {
            if (!callback)
            {
                AlertPrefab.LaunchAlert(message);
                postButton.interactable = true;
            }
            else
            {
                AlertPrefab.LaunchAlert(message);
                StartCoroutine(DelayedBack());
            }
        }));
    }

    private IEnumerator DelayedBack()
    {
        yield return new WaitForSeconds(1.5f);
        FindObjectOfType<PaparazziScript>().Refresh();
        FindObjectOfType<BackEventScript>().GoBack();
    }

    private bool IsFormValide()
    {
        if (m_username.text == "")
        {
            AlertPrefab.LaunchAlert("Le pseudonyme est obligatoire.");
            return false;
        }
        if (m_picturePath == null)
        {
            AlertPrefab.LaunchAlert("Séléctionnez une photo ou prenez-en une nouvelle.");
            return false;
        }
        if (!File.Exists(m_picturePath))
        {
            AlertPrefab.LaunchAlert("Le chemin vers cette photo n'est pas valide.");
            return false;
        }
        return true;
    }

    private IEnumerator PublishCoroutine(Uri url, List<IMultipartFormSection> form, Action<bool, string> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                callback(false, "Échec du chargement. Êtes-vous connecté à internet?");
            }
            else callback(true, "Votre photo a bien été publiée.");
        }
    }

    private void GetPictureCallback(string path)
    {
#if UNITY_EDITOR
        path = Application.persistentDataPath + "/Test/photo-test.jpg"; ;
#endif
        if (path != null)
        {
            m_picturePath = path;

            Texture2D texture = NativeCamera.LoadImageAtPath(m_picturePath);
            if (!texture)
            {
                AlertPrefab.LaunchAlert("Échec du chargement. I&N a t'elle accès au stockage externe? ");
            }
            else
            {
                NativeCamera.ImageProperties props = NativeCamera.GetImageProperties(m_picturePath);
#if UNITY_EDITOR
                props = new NativeCamera.ImageProperties(3968, 2976, "image/jpeg", NativeCamera.ImageOrientation.Normal);
#endif
                float ratio = props.height / (float)props.width;
                float maxSize = picture.rectTransform.rect.width;
                Vector2 newSize = new Vector2(0, 0);
                if (props.height <= props.width)
                {
                    newSize.x = maxSize;
                    newSize.y = maxSize * ratio;
                }
                else
                {
                    newSize.x = maxSize / ratio;
                    newSize.y = maxSize;
                }
                picture.rectTransform.sizeDelta = newSize;

                addPictureTransform.gameObject.SetActive(false);
                picture.texture = texture;
            }
        }
    }

    private void ResetForm()
    {
        picture.texture = null;
        addPictureTransform.gameObject.SetActive(true);
        m_picturePath = null;
        m_username.text = "";
        m_comment.text = "";
        postButton.interactable = true;
    }
}
