using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PhotosGalleryScript : MonoBehaviour
{
    public float margin;
    public GameObject photoPrefab;
    public Animator photosAnimator;
    public Text titleText;

    private ScreenManager m_screenManager;
    private Transform m_content;
    private RectTransform m_contentRectTransform;
    private GalleryManager m_galleryManager;


    protected void Awake()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_content = transform.FindDeepChild("Content");
        m_contentRectTransform = m_content.GetComponent<RectTransform>();
        m_galleryManager = FindObjectOfType<GalleryManager>();
    }

    protected void OnEnable()
    {
        Album album = GalleryManager.gallery.albums[GalleryManager.CurrentAlbumIndex];
        titleText.text = album.label;
        GenerateGallery(album);
    }

    protected void OnDisable()
    {
        foreach (Transform child in m_content)
        {
            Destroy(child.gameObject);
        }
        m_contentRectTransform.sizeDelta = new Vector2(m_contentRectTransform.sizeDelta.x, 0);
        m_contentRectTransform.anchoredPosition = new Vector2(0, 0);
        titleText.text = "Photos";
        Resources.UnloadUnusedAssets();
    }


    private void GenerateGallery(Album album)
    {
        string path = Path.Combine(Application.persistentDataPath + @"/photos/" + Path.GetFileNameWithoutExtension(album.zip_name));
        if (!Directory.Exists(path))
        {
            LoadingSpinScript.Text = "Nous générons votre galerie photos.";
            m_galleryManager.StartLoader();
        }
        Rect scrollAreaSize = transform.GetChild(0).GetComponent<RectTransform>().rect;
        float pictureSide = scrollAreaSize.width / 4.0f;
        float pictureHalfSide = pictureSide / 2.0f;

        m_contentRectTransform.sizeDelta = new Vector2(m_contentRectTransform.sizeDelta.x, (float)Math.Ceiling(album.GetPhotosCount() / 4.0f) * pictureSide);

        PhotoInGalleryScript prefabScript = photoPrefab.GetComponent<PhotoInGalleryScript>();
        RectTransform rectTransform = photoPrefab.GetComponent<RectTransform>();
        RawImage rawImage = photoPrefab.GetComponent<RawImage>();
        rectTransform.sizeDelta = new Vector2(pictureSide - margin, pictureSide - margin);
        prefabScript.photosAnimator = photosAnimator;

        GenerateGalleryInner(0, album.GetPhotosCount());

        void GenerateGalleryInner(int currentIndex, int maxIndex)
        {
            Resources.UnloadUnusedAssets();
            if (currentIndex == maxIndex)
            {
                AlertPrefab.LaunchAlert("Votre galerie photos est prête.");
                m_galleryManager.StopLoader();
                return;
            }

            Photo photo = album.content[currentIndex];
            int column = (int)Math.Floor(currentIndex / 4.0f);
            int row = currentIndex % 4;

            photoPrefab.name = "Photo " + photo.id + "(" + photo.picture_name + ")";
            rectTransform.anchoredPosition = new Vector2(pictureHalfSide + row * pictureSide, -pictureHalfSide - column * pictureSide);

            StartCoroutine(ImageDownloader.GetTextureFromZipAsync(@"/photos/" + Path.GetFileNameWithoutExtension(album.zip_name), "/" + photo.picture_name, new Uri("https://iandn.app/photo/album/" + album.id + "/zip/"), (texture, innerMessage) =>
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
                    LoadingSpinScript.Text = "Nous générons votre galerie photos.";

                    rawImage.texture = CropTexture(texture);

                    Instantiate(photoPrefab, m_content).GetComponent<PhotoInGalleryScript>().PhotoIndex = currentIndex;

                    GenerateGalleryInner(++currentIndex, maxIndex);
                }
            }));
        }

        Texture2D CropTexture(Texture2D texture)
        {
            int cropSize = Math.Min(texture.width, texture.height);
            Texture2D cropedTexture = new Texture2D(cropSize, cropSize);

            cropedTexture.SetPixels(0, 0, cropSize, cropSize, texture.GetPixels((texture.width - cropSize) / 2, (texture.height - cropSize) / 2, cropSize, cropSize, 0), 0);
            cropedTexture.Apply();
            return cropedTexture;
        }
    }
}
