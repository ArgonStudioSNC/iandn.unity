using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhotosAlbumScript : MonoBehaviour
{
    public Transform[] albums;
    public Text titleText;

    private ScreenManager m_screenManager;
    private RawImage[] m_rawImages;
    private Text[] m_titleTexts;
    private Text[] m_contentTexts;
    private Gallery m_gallery;
    private int m_threadsFinishedCount;
    private GalleryManager m_galleryManager;


    protected void Awake()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_rawImages = new RawImage[albums.Length];
        m_titleTexts = new Text[albums.Length];
        m_contentTexts = new Text[albums.Length];
        m_gallery = GalleryManager.gallery;
        m_galleryManager = FindObjectOfType<GalleryManager>();

        for (int i = 0; i < albums.Length; i++)
        {
            m_rawImages[i] = albums[i].FindDeepChild("CoverImage").GetComponent<RawImage>();
            m_rawImages[i].color = new Color(1, 1, 1, 1);
            m_titleTexts[i] = albums[i].Find("TitleText").GetComponent<Text>();
            m_contentTexts[i] = albums[i].Find("ContentText").GetComponent<Text>();
        }
    }

    protected void OnEnable()
    {
        if (titleText) titleText.text = "Photos";

        StartCoroutine(JoinLoadersCoroutine(albums.Length));

        for (int i = 0; i < albums.Length; i++)
        {
            Album album = m_gallery.albums[i];

            m_titleTexts[i].text = album.label;
            m_contentTexts[i].text = album.GetPhotosCount() + " photos";
            Inner(album, i);
        }

        void Inner(Album album, int index)
        {
            albums[index].gameObject.SetActive(true);
            StartCoroutine(ImageDownloader.GetTextureAsync(@"/covers/" + album.cover_name, new Uri("https://iandn.app/photo/album/" + album.id + "/p/"), (texture, innerMessage) =>
            {
                if (!texture)
                {
                    StopAllCoroutines();
                    m_galleryManager.StopLoader();
                    m_screenManager.CloseCurrent();
                    AlertPrefab.LaunchAlert(innerMessage);
                }
                else
                {
                    m_rawImages[index].texture = texture;
                    m_threadsFinishedCount++;
                    LoadingSpinScript.Text = "Nous téléchargeons vos albums photos.\n\n" + m_threadsFinishedCount + "/" + albums.Length;
                }
            }));
        }
    }

    protected void OnDisable()
    {
        foreach (Transform transform in albums)
        {
            transform.gameObject.SetActive(false);
        }
        Resources.UnloadUnusedAssets();
    }


    private IEnumerator JoinLoadersCoroutine(int nbThreads)
    {
        m_threadsFinishedCount = 0;
        do
        {
            yield return null;
        } while (m_threadsFinishedCount < nbThreads);

        AlertPrefab.LaunchAlert("Vos albums photos sont à jour.");
        m_galleryManager.StopLoader();
    }
}
