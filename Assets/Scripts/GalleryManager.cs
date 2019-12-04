using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public class GalleryManager : MonoBehaviour
{
    public Transform photosAlbumTransform;
    public Transform photosGalleryTransform;
    public GameObject loaderPrefab;
    public Transform mainCanvas;

    private ScreenManager m_screenManager;
    private Animator m_photosAlbumAnimator;
    private Animator m_photosGalleryAnimator;

    public UnityWebRequest www { get; set; }


    private static GameObject m_loader;
    public static Gallery gallery { get; private set; }

    public static int CurrentAlbumIndex { get; set; }
    public static int CurrentPhotoIndex { get; set; }


    protected void Awake()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_photosAlbumAnimator = photosAlbumTransform.GetComponent<Animator>();
        m_photosGalleryAnimator = photosGalleryTransform.GetComponent<Animator>();
    }


    public bool StartLoader()
    {
        if (!m_loader)
        {
            Debug.Log("Starting the spinning loader!");
            m_loader = Instantiate(loaderPrefab, mainCanvas);
            return true;
        }
        else return false;
    }

    public bool StopLoader()
    {
        if (m_loader)
        {
            Debug.Log("Stopping the spinning loader!");
            m_loader.GetComponent<LoadingSpinScript>().StopWaitingScreen();
            m_loader = null;
            return true;
        }
        else return false;
    }

    public void OpenAlbum(int index)
    {
        CurrentAlbumIndex = index;
        m_screenManager.OpenPanel(m_photosGalleryAnimator);
    }

    public void LoadGallery()
    {
        if (!StartLoader()) Debug.Log("The gallery loader is already running");
        else
        {
            if (gallery != null)
            {
                LoadingSpinScript.Text = "Nous mettons en place votre galerie photos.";
                m_screenManager.OpenPanel(m_photosAlbumAnimator);
            }
            else
            {
                LoadingSpinScript.Text = "Nous téléchargeons vos albums photos.";
                StartCoroutine(LoadGalleryAsync((tmpGallery, message) =>
                {
                    if (tmpGallery == null)
                    {
                        AlertPrefab.LaunchAlert(message);
                        StopLoader();
                    }
                    else
                    {
                        gallery = tmpGallery;
                        LoadingSpinScript.Text = "Nous mettons en place votre galerie photos.";
                        m_screenManager.OpenPanel(m_photosAlbumAnimator);
                    }
                }));
            }
        }
    }


    private IEnumerator LoadGalleryAsync(Action<Gallery, string> callback)
    {
        Uri uri = new Uri("https://www.iandn.app/photo/album/d");

        using (www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("!Error while downloading the gallery data!");
                yield return null;
                callback(null, "Échec du chargement. Êtes-vous connecté à internet?");
            }
            else
            {
                Gallery tmpGallery = JsonUtility.FromJson<Gallery>("{\"albums\":" + www.downloadHandler.text + "}");
                callback(tmpGallery, "Les données de la galerie sont à jour.");
            }
        }
    }
}

[Serializable]
public class Photo
{
    public uint id;
    public string picture_name;

    public Photo(uint id, string picture_name)
    {
        this.id = id;
        this.picture_name = picture_name;
    }
}

[Serializable]
public class Album
{
    public uint id;
    public string label;
    public string cover_name;
    public string zip_name;
    public List<Photo> content;

    public bool IsAlbumLoaded { get; set; } = false;

    public Album(uint id, string label, string cover_name, string zip_name, List<Photo> content = null)
    {
        this.id = id;
        this.label = label;
        this.cover_name = cover_name;
        this.zip_name = zip_name;
        this.content = (content != null) ? content : new List<Photo>();
    }

    public int GetPhotosCount()
    {
        return content != null ? content.Count : 0;
    }
}

[Serializable]
public class Gallery
{
    public List<Album> albums;

    public Gallery(List<Album> albums = null)
    {
        this.albums = (albums != null) ? albums : new List<Album>();
    }
}