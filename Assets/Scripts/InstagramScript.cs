using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UI;

public class InstagramScript : MonoBehaviour
{
    public MyScrollRect myScrollRect;
    public Transform scrollContent;
    public GameObject itemPrefab;

    private Flux m_flux = new Flux();
    private uint m_numberOfPostsToDownoad;
    private LoadingState m_currentState = LoadingState.Waiting;
    private float m_refreshTimer;

    public UnityWebRequest www { get; set; }

    public enum LoadingState
    {
        Waiting,
        Refreshing,
        ReadyToDownload,
        Downloading
    }

    [Serializable]
    internal class Flux
    {
        public List<Post> posts;

        public Flux()
        {
            posts = new List<Post>();
        }

        public Flux(List<Post> posts)
        {
            this.posts = posts;
        }

        public bool IsEmpty()
        {
            return !posts.Any();
        }
    }

    protected void OnEnable()
    {
        Refresh();
    }

    protected void OnDisable()
    {
        StopAllCoroutines();
        for (int i = 1; i < scrollContent.childCount; i++)
        {
            Destroy(scrollContent.GetChild(i).gameObject);
        }
    }

    protected void Update()
    {
        if (scrollContent.localPosition.y < -20)
        {
            m_refreshTimer += Time.deltaTime;
            if (m_refreshTimer > 0.3f && m_currentState != LoadingState.Refreshing) Refresh();
        }
        else m_refreshTimer = 0f;


        switch (m_currentState)
        {
            case LoadingState.Waiting:
                if (!m_flux.IsEmpty() && myScrollRect.verticalNormalizedPosition < 0.1f)
                {
                    m_numberOfPostsToDownoad = 10;
                    m_currentState = LoadingState.ReadyToDownload;
                }
                break;

            case LoadingState.Refreshing:
                break;

            case LoadingState.ReadyToDownload:
                if (m_flux.IsEmpty() || m_numberOfPostsToDownoad == 0)
                {
                    m_currentState = LoadingState.Waiting;
                }
                else
                {
                    m_currentState = LoadingState.Downloading;
                    LoadPost(m_flux.posts.First());
                }
                break;

            case LoadingState.Downloading:
                break;

            default:
                break;
        }
    }

    public void Refresh()
    {
        m_refreshTimer = 0f;
        if (m_currentState != LoadingState.Refreshing)
        {
            Debug.Log("Refreshing the picture flux...");
            m_currentState = LoadingState.Refreshing;
            StopAllCoroutines();
            StartCoroutine(RefreshFluxAsync());
        }
    }

    private IEnumerator RefreshFluxAsync()
    {
        Uri uri = new Uri("https://www.iandn.app/instagram/d/");
        using (www = UnityWebRequest.Get(uri))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log("!Error while downloading the flux data!");
                AlertPrefab.LaunchAlert("Échec du chargement. Êtes-vous connecté à internet?");
                scrollContent.localPosition = new Vector2(0, 0);
                m_currentState = LoadingState.Waiting;
                yield return null;
            }
            else
            {
                m_flux = JsonUtility.FromJson<Flux>("{\"posts\":" + www.downloadHandler.text + "}");
                Debug.Log("Updating flux data success. Number of posts: " + m_flux.posts.Count);

                for (int i = 1; i < scrollContent.childCount; i++)
                {
                    Destroy(scrollContent.GetChild(i).gameObject);
                }
                scrollContent.localPosition = new Vector2(0, 0);

                AlertPrefab.LaunchAlert("Votre flux Paparazzi est à jour.");
                m_numberOfPostsToDownoad = 10;
                m_currentState = LoadingState.ReadyToDownload;
            }
        }
    }

    private void LoadPost(Post post)
    {
        StartCoroutine(ImageDownloader.GetTextureAsync(@"/posts/" + post.picture_name, new Uri("https://iandn.app/instagram/" + post.id + "/p/thumb"), (texture, message) =>
          {
              if (!texture)
              {
                  AlertPrefab.LaunchAlert(message);
                  m_currentState = LoadingState.Waiting;
              }
              else
              {
                  GameObject newPost = Instantiate(itemPrefab);
                  newPost.GetComponent<PostItemScript>().InitPostItem(post, texture);
                  newPost.transform.SetParent(scrollContent, false);
                  m_flux.posts.RemoveAt(0);
                  m_numberOfPostsToDownoad--;
                  m_currentState = LoadingState.ReadyToDownload;
              }
          }));
    }
}

[Serializable]
public class Post
{
    public uint id;
    public string username;
    public string comment;
    public string picture_name;
    public string created_at;

    public Post(uint id, string username, string comment, string picture_name, string created_at)
    {
        this.id = id;
        this.username = username;
        this.comment = comment;
        this.picture_name = picture_name;
        this.created_at = created_at;
    }

    public DateTime GetTime()
    {
        return DateTime.Parse(created_at);
    }
}