using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LoadingSpinScript : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Transform spinner;
    public float minimumDuration = 1.2f;

    private RawImage m_spinnerImage;
    private bool m_running = false;
    private float m_time = 0.0f;

    protected void Awake()
    {
        m_spinnerImage = GetComponentInChildren<RawImage>();
        videoPlayer.Prepare();
    }

    protected void Update()
    {
        if (m_running) m_time += Time.deltaTime;
    }


    public void StartWaitingScreen()
    {
        StartCoroutine(SpinnerCoroutine());
    }


    public void StopWaitingScreen()
    {
        videoPlayer.isLooping = false;
        StartCoroutine(WaitForVideoPlayer());
    }

    private IEnumerator WaitForVideoPlayer()
    {
        do
        {
            yield return null;
        } while (videoPlayer.isPlaying || m_time < minimumDuration);

        videoPlayer.Stop();
        Destroy(gameObject);
    }

    private IEnumerator SpinnerCoroutine()
    {
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();
        while (videoPlayer.frame < 1)
        {
            yield return null;
        }

        m_spinnerImage.texture = videoPlayer.texture;
        m_running = true;
    }
}
