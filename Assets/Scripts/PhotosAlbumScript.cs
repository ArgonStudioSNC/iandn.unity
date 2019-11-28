using UnityEngine;
using UnityEngine.UI;

public class PhotosAlbumScript : MonoBehaviour
{
    public Transform[] albums;
    public Text titleText;


    private RawImage[] m_rawImages;
    private Text[] m_titleTexts;
    private Text[] m_contentTexts;
    private Gallery m_gallery;


    protected void Awake()
    {
        m_rawImages = new RawImage[albums.Length];
        m_titleTexts = new Text[albums.Length];
        m_contentTexts = new Text[albums.Length];

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
        m_gallery = GalleryManager.gallery;

        for (int i = 0; i < albums.Length; i++)
        {
            m_rawImages[i].texture = m_gallery.albums[i].texture;
            m_titleTexts[i].text = m_gallery.albums[i].label;
            m_contentTexts[i].text = m_gallery.albums[i].GetPhotosCount() + " photos";
        }
    }
}
