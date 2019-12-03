using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhotosScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Transform viewport;
    public Transform draggablePanel;
    public GameObject fullScreenPhotoPrefab;
    public float initialSwipeSpeed = 2.0f;
    public float swipeAccelerationRate = 1000.0f;
    public float minScale = 0.85f;
    public float scaleSpeed = 2.0f;
    public Button downloadButton;
    public float dragSpeedLimit = 1500;


    private float m_viewportWidth;
    private List<Photo> m_albumContent;
    private RectTransform m_prefabRectTransform;
    private RawImage m_prefabRawImage;
    private AspectRatioFitter m_prefabFitter;

    private CanvasScaler m_canvasScaler;
    private RectTransform m_draggableRectTransform;
    private float m_currentPosition;
    private float m_referencePosition;
    private float m_tmpPosition;
    private bool m_photoZoomON = false;
    private float m_currentScale = 1.0f;
    private float m_screenReference;
    private float m_dragSpeed;

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
        m_canvasScaler = GetComponentInParent<CanvasScaler>();
        m_screenReference = m_canvasScaler.referenceResolution.x / (float)Screen.width;

        m_prefabRectTransform.sizeDelta = new Vector2(m_viewportWidth, m_prefabRectTransform.sizeDelta.y);
    }


    protected void OnEnable()
    {
        m_albumContent = new List<Photo>(GalleryManager.gallery.albums[GalleryManager.CurrentAlbumIndex].content);
        m_referencePosition = -GalleryManager.CurrentPhotoIndex * m_viewportWidth;
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
    }

    protected void Update()
    {
        m_draggableRectTransform.localPosition = new Vector2(m_currentPosition, m_draggableRectTransform.localPosition.y);
        PhotosDragged();
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
        Debug.Log(m_dragSpeed);
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

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        m_photoZoomON = true;
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        m_photoZoomON = false;
    }

    public void DownloadPicture()
    {
        downloadButton.interactable = false;
        StartCoroutine(ImageDownloader.GetBytesAsync(new Uri("https://iandn.app/photo/" + m_albumContent[GalleryManager.CurrentPhotoIndex].id + "/p"), (file, message) =>
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
        m_prefabRectTransform.anchoredPosition = new Vector2(photoIndex * m_viewportWidth, 0);

        Texture photoTexture = m_albumContent[photoIndex].texture;
        m_prefabFitter.aspectRatio = photoTexture.width / (float)photoTexture.height;
        m_prefabRawImage.texture = photoTexture;

        fullScreenPhotoPrefab.name = "Photo " + photoIndex + "(id " + m_albumContent[photoIndex].id + ")";
        return Instantiate(fullScreenPhotoPrefab, draggablePanel);
    }


    private IEnumerator SwipeAnimation()
    {
        float swipeSpeed = initialSwipeSpeed * m_screenReference;
        m_referencePosition = -GalleryManager.CurrentPhotoIndex * m_viewportWidth;
        m_tmpPosition = m_currentPosition;

        if (m_tmpPosition > m_referencePosition)
        {
            swipeSpeed = -swipeSpeed;
            for (m_tmpPosition = m_currentPosition; m_tmpPosition > m_referencePosition; m_tmpPosition += swipeSpeed * Time.deltaTime)
            {
                swipeSpeed -= Time.deltaTime * swipeAccelerationRate;
                m_currentPosition = Mathf.Clamp(m_tmpPosition, m_referencePosition - m_viewportWidth, m_referencePosition + m_viewportWidth);
                yield return null;
            }
            m_currentPosition = m_referencePosition;
        }
        else
        {
            for (m_tmpPosition = m_currentPosition; m_tmpPosition < m_referencePosition; m_tmpPosition += swipeSpeed * Time.deltaTime)
            {
                swipeSpeed += Time.deltaTime * swipeAccelerationRate;
                m_currentPosition = Mathf.Clamp(m_tmpPosition, m_referencePosition - m_viewportWidth, m_referencePosition + m_viewportWidth);
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
    }

    private void PhotosDragged()
    {
        float minValue = minScale;
        float maxValue = 1.0f;
        if (m_photoZoomON && m_currentScale > minValue)
        {
            m_currentScale = Mathf.Clamp(m_currentScale - (scaleSpeed * m_screenReference * Time.deltaTime), minValue, maxValue);
            Apply();
        }
        else if (!m_photoZoomON && m_currentScale < maxValue)
        {
            m_currentScale = Mathf.Clamp(m_currentScale + (scaleSpeed * m_screenReference * Time.deltaTime), minValue, maxValue);
            Apply();
        }

        void Apply()
        {
            Vector3 scaleVector = new Vector3(m_currentScale, m_currentScale, m_currentScale);
            Color color = new Color(1, 1, 1, m_currentScale);

            if (currentPhoto) currentPhoto.GetComponentInChildren<RectTransform>().localScale = scaleVector;
            if (nextPhoto) nextPhoto.GetComponentInChildren<RectTransform>().localScale = scaleVector;
            if (previousPhoto) previousPhoto.GetComponentInChildren<RectTransform>().localScale = scaleVector;
            if (currentPhoto) currentPhoto.GetComponentInChildren<RawImage>().color = color;
            if (nextPhoto) nextPhoto.GetComponentInChildren<RawImage>().color = color;
            if (previousPhoto) previousPhoto.GetComponentInChildren<RawImage>().color = color;
        }
    }

    private bool IndexExist(int i)
    {
        return (i >= 0 && i < m_albumContent.Count);
    }
}
