using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PhotosGalleryScript : MonoBehaviour
{
    public float margin;
    public GameObject photoPrefab;
    public Animator photosAnimator;

    private GalleryManager m_galleryManager;
    private ScreenManager m_screenManager;
    private Transform m_content;
    private RectTransform m_contentRectTransform;


    protected void Awake()
    {
        m_galleryManager = FindObjectOfType<GalleryManager>();
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_content = transform.FindDeepChild("Content");
        m_contentRectTransform = m_content.GetComponent<RectTransform>();
    }


    protected void OnEnable()
    {
        StartCoroutine(m_galleryManager.LoadAlbumAsync(GalleryManager.CurrentAlbumIndex, (album, message) =>
        {
            if (album == null)
            {
                AlertPrefab.LaunchAlert(message);
                m_screenManager.CloseCurrent();
            }
            else
            {
                AlertPrefab.LaunchAlert(message);
                StartCoroutine(GenerateGallery(album));
            }
        }));
    }

    protected void OnDisable()
    {
        foreach (Transform child in m_content)
        {
            Destroy(child.gameObject);
        }
        m_contentRectTransform.sizeDelta = new Vector2(m_contentRectTransform.sizeDelta.x, 0);
    }


    private IEnumerator GenerateGallery(Album album)
    {
        yield return null;
        Rect scrollAreaSize = transform.GetChild(0).GetComponent<RectTransform>().rect;
        float pictureSide = scrollAreaSize.width / 4.0f;
        float pictureHalfSide = pictureSide / 2.0f;

        m_contentRectTransform.sizeDelta = new Vector2(m_contentRectTransform.sizeDelta.x, (float)(Math.Floor(album.GetPhotosCount() / 4.0f) + 1) * pictureSide);

        PhotoInGalleryScript prefabScript = photoPrefab.GetComponent<PhotoInGalleryScript>();
        RectTransform rectTransform = photoPrefab.GetComponent<RectTransform>();
        RawImage rawImage = photoPrefab.GetComponent<RawImage>();
        rectTransform.sizeDelta = new Vector2(pictureSide - margin, pictureSide - margin);
        prefabScript.photosAnimator = photosAnimator;

        for (int i = 0; i < album.GetPhotosCount(); i++)
        {
            Photo photo = album.content[i];
            int column = (int)Math.Floor(i / 4.0f);
            int row = i % 4;

            photoPrefab.name = "Photo " + photo.id + "(" + photo.picture_name + ")";
            rectTransform.anchoredPosition = new Vector2(pictureHalfSide + row * pictureSide, -pictureHalfSide - column * pictureSide);
            rawImage.texture = CropTexture(photo.texture);

            Instantiate(photoPrefab, m_content).GetComponent<PhotoInGalleryScript>().PhotoIndex = i;
        }
        AlertPrefab.LaunchAlert("La galerie a été générée avec succès.");
        m_galleryManager.StopLoader();
    }


    private Texture2D CropTexture(Texture2D texture)
    {
        int cropSize = Math.Min(texture.width, texture.height);
        Texture2D cropedTexture = new Texture2D(cropSize, cropSize);

        cropedTexture.SetPixels(0, 0, cropSize, cropSize, texture.GetPixels((texture.width - cropSize) / 2, (texture.height - cropSize) / 2, cropSize, cropSize, 0), 0);
        cropedTexture.Apply();
        return cropedTexture;
    }

}
