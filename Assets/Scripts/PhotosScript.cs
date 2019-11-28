using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotosScript : MonoBehaviour
{
    public Transform viewport;
    public Transform draggablePanel;
    public GameObject fullScreenPhotoPrefab;


    private float m_viewportWidth;
    private List<Photo> m_albumContent;
    private RectTransform m_prefabRectTransform;
    private RawImage m_prefabRawImage;
    private AspectRatioFitter m_prefabFitter;
    private RectTransform m_draggableRectTransform;

    private GameObject previousPreviousPhoto;
    private GameObject previousPhoto;
    private GameObject currentPhoto;
    private GameObject nextPhoto;
    private GameObject nextNextPhoto;


    protected void Awake()
    {
        m_prefabRectTransform = fullScreenPhotoPrefab.GetComponent<RectTransform>();
        m_prefabRawImage = fullScreenPhotoPrefab.GetComponentInChildren<RawImage>();
        m_prefabFitter = fullScreenPhotoPrefab.GetComponentInChildren<AspectRatioFitter>();
        m_viewportWidth = viewport.GetComponent<RectTransform>().rect.width;
        m_draggableRectTransform = draggablePanel.GetComponent<RectTransform>();

        m_prefabRectTransform.sizeDelta = new Vector2(m_viewportWidth, m_prefabRectTransform.sizeDelta.y);
    }


    protected void OnEnable()
    {
        m_albumContent = new List<Photo>(GalleryManager.gallery.albums[GalleryManager.CurrentAlbumIndex].content);
        m_draggableRectTransform.anchoredPosition = new Vector2(-GalleryManager.CurrentPhotoIndex * m_viewportWidth, 0);

        currentPhoto = InstantiatePhoto(GalleryManager.CurrentPhotoIndex);
    }


    protected void OnDisable()
    {
        foreach (Transform child in draggablePanel)
        {
            Destroy(child.gameObject);
        }
    }


    private GameObject InstantiatePhoto(int photoIndex)
    {
        m_prefabRectTransform.anchoredPosition = new Vector2(m_viewportWidth / 2.0f + photoIndex * m_viewportWidth, 0);

        Texture photoTexture = m_albumContent[photoIndex].texture;
        m_prefabFitter.aspectRatio = photoTexture.width / (float)photoTexture.height;
        m_prefabRawImage.texture = photoTexture;

        fullScreenPhotoPrefab.name = "Photo " + photoIndex + "(id " + m_albumContent[photoIndex].id + ")";
        return Instantiate(fullScreenPhotoPrefab, draggablePanel);
    }

}
