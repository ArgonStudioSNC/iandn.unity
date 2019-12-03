using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

public class GalleryManager : MonoBehaviour
{
    public GameObject loaderPrefab;
    public Transform mainCanvas;
    public Transform photosAlbumTransform;
    public Transform photosGalleryTransform;


    private ScreenManager m_screenManager;
    private Animator m_photosAlbumAnimator;
    private Animator m_photosGalleryAnimator;
    private LoadingState m_currentState = LoadingState.Idle;
    private GameObject m_loader;
    private int m_threadsFinishedCount;

    public UnityWebRequest www { get; set; }
    public static Gallery gallery { get; private set; }

    public static int CurrentAlbumIndex { get; set; }
    public static int CurrentPhotoIndex { get; set; }


    public enum LoadingState
    {
        Idle,
        Loading,
        Error
    }


    protected void Awake()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_photosAlbumAnimator = photosAlbumTransform.GetComponent<Animator>();
        m_photosGalleryAnimator = photosGalleryTransform.GetComponent<Animator>();
    }

    protected void Update()
    {
        switch (m_currentState)
        {
            case LoadingState.Idle:
                break;

            case LoadingState.Loading:
                break;

            case LoadingState.Error:
                StopAllCoroutines();
                StopLoader();
                break;

            default:
                break;
        }
    }

    public void StopLoader()
    {
        Debug.Log("Stoping the loader");
        m_currentState = LoadingState.Idle;
        if (m_loader)
        {
            m_loader.GetComponent<LoadingSpinScript>().StopWaitingScreen();
            m_loader = null;
        }
    }

    public void OpenAlbum(int index)
    {
        CurrentAlbumIndex = index;
        m_screenManager.OpenPanel(m_photosGalleryAnimator);
    }

    public void LoadGallery()
    {
        if (m_currentState != LoadingState.Idle)
        {
            Debug.Log("The gallery loader is already running");
        }
        else if (gallery != null)
        {
            m_screenManager.OpenPanel(m_photosAlbumAnimator);
        }
        else
        {
            StartLoader();
            LoadGallerySubroutine();
        }
    }


    public IEnumerator LoadAlbumAsync(int albumIndex, Action<Album, string> callback)
    {
        StartLoader();
        if (gallery == null)
        {
            yield return null;
            callback(null, "Erreur. La galerie ne semble pas chargée.");
        }

        if (gallery.albums[albumIndex].IsAlbumLoaded)
        {
            callback(gallery.albums[albumIndex], "L'album a été chargé depuis la mémoire.");
        }
        else
        {
            Uri uri = new Uri("https://www.iandn.app/photo/album/" + albumIndex + "/zip/");

            using (www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log("!Error while downloading the album zip!");
                    m_currentState = LoadingState.Error;
                    yield return null;
                    callback(null, "Échec du chargement. Êtes-vous connecté à internet?");
                }
                else
                {
                    byte[] zipFileData = www.downloadHandler.data;
                    string zipFilePath = Path.GetFileNameWithoutExtension(gallery.albums[albumIndex].zip_name) + "/";

                    using (MemoryStream albumFileStream = new MemoryStream())
                    {
                        albumFileStream.Write(zipFileData, 0, zipFileData.Length);
                        albumFileStream.Flush();
                        albumFileStream.Seek(0, SeekOrigin.Begin);

                        ZipArchive zipArchive = new ZipArchive(albumFileStream);

                        foreach (Photo photo in gallery.albums[albumIndex].content)
                        {
                            ZipArchiveEntry archiveEntry = zipArchive.GetEntry(zipFilePath + photo.picture_name);

                            Texture2D photoTexture = new Texture2D(2, 2); // the size doesn't mather
                            Stream photoStream = archiveEntry.Open();
                            byte[] bytes;
                            using (MemoryStream photoFileStream = new MemoryStream())
                            {
                                photoStream.CopyTo(photoFileStream);
                                bytes = photoFileStream.ToArray();
                            }

                            photoTexture.LoadImage(bytes);
                            photo.texture = photoTexture;
                        }
                    }

                    gallery.albums[albumIndex].IsAlbumLoaded = true;
                    callback(gallery.albums[albumIndex], "L'album a été téléchargé.");
                }
            }
        }
    }

    private void StartLoader()
    {
        if (m_currentState == LoadingState.Idle)
        {
            Debug.Log("Starting the loader");
            m_currentState = LoadingState.Loading;
            m_loader = Instantiate(loaderPrefab, mainCanvas);
        }
    }

    private void LoadGallerySubroutine()
    {
        StartCoroutine(LoadGalleryAsync((tmpGallery, message) =>
        {
            if (tmpGallery == null)
            {
                AlertPrefab.LaunchAlert(message);
                m_currentState = LoadingState.Error;
            }
            else
            {
                gallery = tmpGallery;
                m_threadsFinishedCount = 0;
                StartCoroutine(JoinLoadersCoroutine(gallery.albums.Count));
                foreach (Album album in gallery.albums)
                {
                    StartCoroutine(ImageDownloader.GetTextureAsync(@"/covers/" + album.cover_name, new Uri("https://iandn.app/photo/album/" + album.id + "/p/"), (texture, innerMessage) =>
                    {
                        if (!texture)
                        {
                            StopAllCoroutines();
                            gallery = null;
                            AlertPrefab.LaunchAlert(innerMessage);
                            m_currentState = LoadingState.Error;
                        }
                        else
                        {
                            album.texture = texture;
                            m_threadsFinishedCount++;
                        }
                    }));
                }
            }
        }));
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

    private IEnumerator JoinLoadersCoroutine(int nbThreads)
    {
        do
        {
            yield return new WaitForSeconds(0.3f);
        } while (m_threadsFinishedCount < nbThreads);

        StopLoader();
        AlertPrefab.LaunchAlert("Votre galerie photos est à jour.");
        m_screenManager.OpenPanel(m_photosAlbumAnimator);
    }

}

[Serializable]
public class Photo
{
    public uint id;
    public string picture_name;
    public Texture2D texture;

    public Photo(uint id, string picture_name, Texture2D texture = null)
    {
        this.id = id;
        this.picture_name = picture_name;
        this.texture = texture;
    }
}

[Serializable]
public class Album
{
    public uint id;
    public string label;
    public string cover_name;
    public Texture2D texture;
    public string zip_name;
    public List<Photo> content;

    public bool IsAlbumLoaded { get; set; } = false;

    public Album(uint id, string label, string cover_name, string zip_name, List<Photo> content = null, Texture2D texture = null)
    {
        this.id = id;
        this.label = label;
        this.cover_name = cover_name;
        this.zip_name = zip_name;
        this.content = (content != null) ? content : new List<Photo>();
        this.texture = texture;
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