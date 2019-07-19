using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PostItemScript : MonoBehaviour
{
    public Text usernameText;
    public Transform pictureObject;
    public Text commentText;
    public Text timestampText;
    public Button downloadButton;

    private Post m_post;

    public void InitPostItem(Post post, Texture2D texture)
    {
        m_post = post;
        usernameText.text = post.username;
        if (post.comment == null || post.comment == "") Destroy(commentText);
        else commentText.text = post.comment;
        pictureObject.GetComponent<AspectRatioFitter>().aspectRatio = texture.width / (float)texture.height;
        pictureObject.GetComponent<RawImage>().texture = texture;
        timestampText.text = ComputeTimeString(post.GetTime());
    }

    public void DownloadPicture()
    {
        downloadButton.interactable = false;
        StartCoroutine(ImageDownloader.GetBytesAsync(new Uri("https://iandn.app/instagram/" + m_post.id + "/p"), (file, message) =>
        {
            if (file == null)
            {
                AlertPrefab.LaunchAlert(message);
                downloadButton.interactable = true;
            }
            else
            {
                if (NativeGallery.IsMediaPickerBusy())
                {
                    AlertPrefab.LaunchAlert("MediaPicker est déjà en cours d'utilisation.");
                    downloadButton.interactable = true;
                    return;
                }

                if (NativeGallery.CheckPermission() == NativeGallery.Permission.Granted) SavePicture(file);
                else if (NativeGallery.CheckPermission() == NativeGallery.Permission.ShouldAsk &&
                        NativeGallery.RequestPermission() == NativeGallery.Permission.Granted) SavePicture(file);
            }
        }));
    }

    private void SavePicture(byte[] file)
    {
        NativeGallery.SaveImageToGallery(file, "I&N", "Image du flux {0}.jpg", callback =>
        {
            if (callback == null) AlertPrefab.LaunchAlert("La photo de \"" + m_post.username + "\" a été ajoutée à votre galerie.");

            else AlertPrefab.LaunchAlert("Échec de l'enregistrement. I&N a t'elle accès au stockage externe?");

            downloadButton.interactable = true;
        });
    }

    private string ComputeTimeString(DateTime timestamp)
    {
        string stringBuilder = "";
        TimeSpan timeSpan = DateTime.Now.Subtract(timestamp);

        if (timeSpan.Days != 0)
        {
            stringBuilder = "il y à " + timeSpan.Days;
            stringBuilder += timeSpan.Days == 1 ? " jour" : " jours";
        }
        else if (timeSpan.Hours != 0)
        {
            stringBuilder = "il y à " + timeSpan.Hours;
            stringBuilder += timeSpan.Hours == 1 ? " heure" : " heures";
        }
        else if (timeSpan.Minutes != 0)
        {
            stringBuilder = "il y à " + timeSpan.Minutes;
            stringBuilder += timeSpan.Minutes == 1 ? " minute" : " minutes";
        }
        else if (timeSpan.Seconds != 0 && timeSpan.Seconds > 10)
        {
            stringBuilder = "il y à " + timeSpan.Seconds;
            stringBuilder += timeSpan.Seconds == 1 ? " seconde" : " secondes";
        }
        else
        {
            stringBuilder = "à l'instant";
        }

        return stringBuilder;
    }

    private IEnumerator DownloadPictureAsync()
    {
        return null;
    }
}
