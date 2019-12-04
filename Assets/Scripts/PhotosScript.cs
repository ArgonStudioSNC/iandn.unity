using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhotosScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform viewport;
    public Transform draggablePanel;
    public GameObject fullScreenPhotoPrefab;
    public float minimumSwipeSpeed = 1000.0f;
    public float swipeAccelerationRate = 1000.0f;
    public float minScale = 0.85f;
    public float scaleSpeed = 2.0f;
    public Button downloadButton;
    public float dragSpeedLimit = 1500;
    public float spacing = 2.0f;


    private ScreenManager m_screenManager;
    private Album m_album;
    private float m_viewportWidth;
    private RectTransform m_prefabRectTransform;
    private RawImage m_prefabRawImage;
    private AspectRatioFitter m_prefabFitter;

    private CanvasScaler m_canvasScaler;
    private RectTransform m_draggableRectTransform;
    private float m_currentPosition;
    private float m_referencePosition;
    private float m_tmpPosition;
    private float m_screenReference;
    private float m_dragSpeed;

    private GameObject previousPreviousPhoto;
    private GameObject previousPhoto;
    private GameObject currentPhoto;
    private GameObject nextPhoto;
    private GameObject nextNextPhoto;


    protected void Awake()
    {
        m_screenManager = FindObjectOfType<ScreenManager>();
        m_prefabRectTransform = fullScreenPhotoPrefab.GetComponent<RectTransform>();
        m_prefabRawImage = fullScreenPhotoPrefab.GetComponentInChildren<RawImage>();
        m_prefabFitter = fullScreenPhotoPrefab.GetComponentInChildren<AspectRatioFitter>();
        m_viewportWidth = viewport.GetComponent<RectTransform>().rect.width;
        m_draggableRectTransform = draggablePanel.GetComponent<RectTransform>();
        m_canvasScaler = GetComponentInParent<CanvasScaler>();
        m_screenReference = m_canvasScaler.referenceResolution.x / (float)Screen.width;

        m_prefabRectTransform.sizeDelta = new Vector2(m_viewportWidth, m_prefabRectTransform.sizeDelta.y);
    }


    protected void OnEnable()
    {
        m_album = GalleryManager.gallery.albums[GalleryManager.CurrentAlbumIndex];
        m_referencePosition = -GalleryManager.CurrentPhotoIndex * (m_viewportWidth + spacing);
        m_draggableRectTransform.localPosition = new Vector2(m_referencePosition, m_draggableRectTransform.localPosition.y);

        m_currentPosition = m_referencePosition;
        StartCoroutine(Populate());
    }


    protected void OnDisable()
    {
        StopAllCoroutines();
        foreach (Transform child in draggablePanel)
        {
            Destroy(child.gameObject);
        }
        Resources.UnloadUnusedAssets();
    }

    protected void Update()
    {
        m_draggableRectTransform.localPosition = new Vector2(m_currentPosition, m_draggableRectTransform.localPosition.y);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        m_tmpPosition = m_currentPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        m_tmpPosition += (m_screenReference * eventData.delta.x);
        m_dragSpeed = (m_currentPosition - m_tmpPosition) / Time.deltaTime;

        float minX = m_referencePosition - (nextPhoto ? m_viewportWidth : (m_viewportWidth / 5.0f));
        float maxX = m_referencePosition + (previousPhoto ? m_viewportWidth : (m_viewportWidth / 5.0f));

        m_currentPosition = Mathf.Clamp(m_tmpPosition, minX, maxX);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (m_dragSpeed > dragSpeedLimit)
        {
            Right();
        }
        else if (m_dragSpeed < -dragSpeedLimit)
        {
            Left();
        }
        else if (m_currentPosition < m_referencePosition - m_viewportWidth / 2.0f)
        {
            Right();
        }
        else if (m_currentPosition > m_referencePosition + m_viewportWidth / 2.0f)
        {
            Left();
        }
        else
        {
            Debug.Log("Go to current photo");
            StartCoroutine(SwipeAnimation());
        }
    }

    public void DownloadPicture()
    {
        downloadButton.interactable = false;
        StartCoroutine(ImageDownloader.GetBytesAsync(new Uri("https://iandn.app/photo/" + m_album.content[GalleryManager.CurrentPhotoIndex].id + "/p"), (file, message) =>
        {
            if (file == null) AlertPrefab.LaunchAlert(message);
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
                else AlertPrefab.LaunchAlert("L'application n'est pas autorisée à acceder à la mémoire externe.");
            }
            downloadButton.interactable = true;
        }));
    }

    private void Right()
    {
        if (IndexExist(GalleryManager.CurrentPhotoIndex + 1))
        {
            Debug.Log("Go to next photo");
            GalleryManager.CurrentPhotoIndex += 1;
            Destroy(previousPreviousPhoto);
            previousPreviousPhoto = previousPhoto;
            previousPhoto = currentPhoto;
            currentPhoto = nextPhoto;
            nextPhoto = nextNextPhoto;
            nextNextPhoto = null;

            StartCoroutine(Populate());
            StartCoroutine(SwipeAnimation());
        }
        else
        {
            Debug.Log("Go to current photo");
            StartCoroutine(SwipeAnimation());
        }
    }

    private void Left()
    {
        if (IndexExist(GalleryManager.CurrentPhotoIndex - 1))
        {
            Debug.Log("Go to previous photo");
            GalleryManager.CurrentPhotoIndex -= 1;
            Destroy(nextNextPhoto);
            nextNextPhoto = nextPhoto;
            nextPhoto = currentPhoto;
            currentPhoto = previousPhoto;
            previousPhoto = previousPreviousPhoto;
            previousPreviousPhoto = null;

            StartCoroutine(Populate());
            StartCoroutine(SwipeAnimation());
        }
        else
        {
            Debug.Log("Go to current photo");
            StartCoroutine(SwipeAnimation());
        }
    }

    private void SavePicture(byte[] file)
    {
        NativeGallery.SaveImageToGallery(file, "I&N", "Photo téléchargée {0}.jpg", callback =>
        {
            if (callback == null) AlertPrefab.LaunchAlert("La photo de a été ajoutée à votre galerie.");

            else AlertPrefab.LaunchAlert("Échec de l'enregistrement. I&N a t'elle accès au stockage externe?");
        });
    }

    private GameObject InstantiatePhoto(int photoIndex)
    {
        Photo photo = m_album.content[photoIndex];
        m_prefabRectTransform.anchoredPosition = new Vector2(photoIndex * (m_viewportWidth + spacing), 0);
        fullScreenPhotoPrefab.name = "Photo " + photoIndex + "(id " + m_album.content[photoIndex].id + ")";

        GameObject photoInstance = Instantiate(fullScreenPhotoPrefab, draggablePanel);
        SetTextureInner(photoInstance, photo);
        return photoInstance;

        void SetTextureInner(GameObject photoGo, Photo p)
        {
            StartCoroutine(ImageDownloader.GetTextureFromZipAsync(@"/photos/" + Path.GetFileNameWithoutExtension(m_album.zip_name), "/" + p.picture_name, new Uri("https://iandn.app/photo/album/" + m_album.id + "/zip/"), (photoTexture, innerMessage) =>
            {
                if (!photoTexture)
                {
                    StopAllCoroutines();
                    m_screenManager.CloseCurrent();
                    AlertPrefab.LaunchAlert(innerMessage);
                }
                else
                {
                    photoGo.GetComponentInChildren<AspectRatioFitter>().aspectRatio = photoTexture.width / (float)photoTexture.height;
                    photoGo.GetComponentInChildren<RawImage>().texture = photoTexture;
                }
            }));
        }
    }


    private IEnumerator SwipeAnimation()
    {
        float swipeSpeed = Math.Max(minimumSwipeSpeed * m_screenReference, Math.Abs(m_dragSpeed));

        m_referencePosition = -GalleryManager.CurrentPhotoIndex * (m_viewportWidth + spacing);
        m_tmpPosition = m_currentPosition;

        if (m_tmpPosition > m_referencePosition)
        {
            swipeSpeed = -swipeSpeed;
            for (m_tmpPosition = m_currentPosition; m_tmpPosition > m_referencePosition; m_tmpPosition += swipeSpeed * Time.deltaTime)
            {
                swipeSpeed -= Time.deltaTime * swipeAccelerationRate * m_screenReference;
                m_currentPosition = Mathf.Clamp(m_tmpPosition, m_referencePosition - (m_viewportWidth + spacing), m_referencePosition + (m_viewportWidth + spacing));
                yield return null;
            }
            m_currentPosition = m_referencePosition;
        }
        else
        {
            for (m_tmpPosition = m_currentPosition; m_tmpPosition < m_referencePosition; m_tmpPosition += swipeSpeed * Time.deltaTime)
            {
                swipeSpeed += Time.deltaTime * swipeAccelerationRate * m_screenReference;
                m_currentPosition = Mathf.Clamp(m_tmpPosition, m_referencePosition - (m_viewportWidth + spacing), m_referencePosition + (m_viewportWidth + spacing));
                yield return null;
            }
            m_currentPosition = m_referencePosition;
        }
    }

    private IEnumerator Populate()
    {
        yield return null;

        int index = GalleryManager.CurrentPhotoIndex;
        if (!currentPhoto) currentPhoto = IndexExist(index) ? InstantiatePhoto(index) : null;
        if (!nextPhoto) nextPhoto = IndexExist(index + 1) ? InstantiatePhoto(index + 1) : null;
        if (!previousPhoto) previousPhoto = IndexExist(index - 1) ? InstantiatePhoto(index - 1) : null;
        if (!nextNextPhoto) nextNextPhoto = IndexExist(index + 2) ? InstantiatePhoto(index + 2) : null;
        if (!previousPreviousPhoto) previousPreviousPhoto = IndexExist(index - 2) ? InstantiatePhoto(index - 2) : null;
        Resources.UnloadUnusedAssets();
    }

    private bool IndexExist(int i)
    {
        return (i >= 0 && i < m_album.content.Count);
    }
}
