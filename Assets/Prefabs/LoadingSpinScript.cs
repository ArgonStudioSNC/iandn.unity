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
        StartCoroutine(SpinnerCoroutine());
    }


    protected void Update()
    {
        if (m_running) m_time += Time.deltaTime;
    }


    public void StopWaitingScreen()
    {
        StartCoroutine(WaitForVideoPlayer());
    }

    private IEnumerator WaitForVideoPlayer()
    {
        do
        {
            yield return null;
        } while (m_time < minimumDuration);
        videoPlayer.isLooping = false;

        while (videoPlayer.isPlaying)
        {
            yield return null;
        }
        videoPlayer.Stop();

        Destroy(gameObject);
    }

    private IEnumerator SpinnerCoroutine()
    {
        do
        {
            yield return null;
        } while (!videoPlayer.isPrepared);
        videoPlayer.Play();

        do
        {
            yield return null;
        } while (videoPlayer.frame < 1);
        m_spinnerImage.texture = videoPlayer.texture;

        m_running = true;
    }
}
