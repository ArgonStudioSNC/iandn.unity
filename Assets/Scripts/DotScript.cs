using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class DotScript : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float duration = 0.2f;
    public Transform cursor;
    public VideoPlayer videoPlayer;
    public Texture staticFrame;

    private RawImage m_rawImage;
    private float m_thisAnchor;
    private float m_cursorPosition;
    private bool m_animationIsOn = false;
    private bool m_lock = false;

    protected void Awake()
    {
        m_rawImage = transform.GetComponentInChildren<RawImage>();
        m_thisAnchor = transform.GetComponent<RectTransform>().anchorMax.y;
        videoPlayer.Prepare();
    }

    protected void OnDisable()
    {
        m_rawImage.texture = staticFrame;
        videoPlayer.isLooping = false;
        videoPlayer.Stop();
        m_animationIsOn = false;
        m_lock = false;
    }

    protected void Update()
    {
        if (!m_lock)
        {
            if (IsInRange())
            {
                if (!m_animationIsOn)
                {
                    StartCoroutine(StartVideoPlayer());
                }
            }
            else
            {
                if (m_animationIsOn)
                {
                    m_lock = true;
                    m_animationIsOn = false;
                    videoPlayer.isLooping = false;
                    videoPlayer.loopPointReached += EndReached;
                }
            }
        }
    }

    private bool IsInRange()
    {
        m_cursorPosition = cursor.GetComponent<RectTransform>().anchorMax.y;
        return m_cursorPosition <= m_thisAnchor && m_cursorPosition > m_thisAnchor - duration;
    }

    private IEnumerator StartVideoPlayer()
    {
        m_lock = true;
        m_animationIsOn = true;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
        while (videoPlayer.frame < 1)
        {
            yield return null;
        }
        m_rawImage.texture = videoPlayer.texture;
        m_lock = false;
    }

    private void EndReached(VideoPlayer vp)
    {
        vp.Stop();
        m_rawImage.texture = staticFrame;
        m_lock = false;
    }
}
