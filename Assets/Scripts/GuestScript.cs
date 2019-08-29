using System;
using System.Collections;
using System.Collections.Generic;
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

    [Serializable]
    internal class GuestList
    {
        public List<Guest> guests;

        public GuestList(List<Guest> guests)
        {
            this.guests = guests;
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

    private GuestList m_guests;

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
            StartCoroutine(GetNextGuestCoroutine());
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
            StartCoroutine(GetNextGuestCoroutine());

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
        yield return StartCoroutine(GetNextGuestCoroutine());
        if (m_currentState == LoadingState.Success) NextGuest();
    }

    private IEnumerator GetNextGuestCoroutine()
    {
        Uri uri = new Uri("https://www.iandn.app/guest/d/");

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
                m_guests = JsonUtility.FromJson<GuestList>("{\"guests\":" + www.downloadHandler.text + "}");

                m_nextGuest = NextUnseenGuest();
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

    private Guest NextUnseenGuest()
    {
        string key = "guest_id";
        if (!PlayerPrefs.HasKey(key)) PlayerPrefs.SetInt(key, 0);
        int currentID = PlayerPrefs.GetInt(key);

        foreach (Guest guest in m_guests.guests)
        {
            if (guest.id > currentID)
            {
                PlayerPrefs.SetInt(key, (int)guest.id);
                return guest;
            }
        }

        PlayerPrefs.SetInt(key, 0);

        return NextUnseenGuest();
    }
}
