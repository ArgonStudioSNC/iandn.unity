using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GuestScript : MonoBehaviour
{
    public enum LoadingState
    {
        Downloading,
        Success,
        Error
    }

    [Serializable]
    internal class Guest
    {
        public uint id;
        public string fullname;
        public string description;
        public string picture_name;
        public Texture2D picture { get; set; }

        public Guest(uint id, string fullname, string description, string picture_name)
        {
            this.id = id;
            this.fullname = fullname;
            this.description = description;
            this.picture_name = picture_name;
        }
    }

    public Text fullname;
    public Text description;

    public UnityWebRequest www { get; set; }

    private LoadingState m_currentState = LoadingState.Error;
    private Guest m_currentGuest = null;
    private Guest m_nextGuest = null;

    private RawImage m_rawImage;
    private Button m_button;

    protected void Awake()
    {
        m_rawImage = GetComponentInChildren<RawImage>();
        m_button = GetComponentInChildren<Button>();
    }

    protected void OnEnable()
    {
        if (m_currentGuest == null)
        {
            StartCoroutine(Startup());
        }
        else
        {
            StartCoroutine(GetRandomGuestCoroutine());
        }
    }

    protected void Update()
    {
        switch (m_currentState)
        {
            case LoadingState.Downloading:
                m_button.interactable = false;
                break;

            case LoadingState.Error:
                m_nextGuest = null;
                m_button.interactable = true;
                break;

            case LoadingState.Success:
                m_button.interactable = true;
                break;

            default:
                break;
        }
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
    }

    public void NextGuest()
    {
        if (m_currentState == LoadingState.Success)
        {
            m_currentGuest = m_nextGuest;

            m_currentState = LoadingState.Downloading;
            StartCoroutine(GetRandomGuestCoroutine());

            fullname.text = m_currentGuest.fullname;
            description.text = m_currentGuest.description;
            m_rawImage.texture = m_currentGuest.picture;
        }
        else if (m_currentState == LoadingState.Error)
        {
            StartCoroutine(Startup());
        }
    }

    private IEnumerator Startup()
    {
        m_currentState = LoadingState.Downloading;
        yield return StartCoroutine(GetRandomGuestCoroutine());
        if (m_currentState == LoadingState.Success) NextGuest();
    }

    private IEnumerator GetRandomGuestCoroutine()
    {
        Uri uri = new Uri("https://www.iandn.app/guest/random/");

        using (www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                AlertPrefab.LaunchAlert("Échec du chargement. Êtes-vous connecté à internet?");
                m_currentState = LoadingState.Error;
                yield return null;
            }
            else
            {
                Guest guest = JsonUtility.FromJson<Guest>(www.downloadHandler.text);
                if (m_currentGuest != null && guest.id == m_currentGuest.id)
                {
                    yield return StartCoroutine(GetRandomGuestCoroutine());
                }
                else
                {
                    m_nextGuest = guest;

                    string picturePath = @"/guests/" + m_nextGuest.picture_name;
                    Uri pictureUri = new Uri("https://www.iandn.app/guest/" + m_nextGuest.id + "/p/");

                    yield return StartCoroutine(ImageDownloader.GetTextureAsync(picturePath, pictureUri, (texture, message) =>
                    {
                        if (!texture)
                        {
                            AlertPrefab.LaunchAlert(message);
                            m_currentState = LoadingState.Error;
                        }
                        else
                        {
                            m_nextGuest.picture = texture;
                            m_currentState = LoadingState.Success;
                        }
                    }));
                }
            }
        }
    }
}
