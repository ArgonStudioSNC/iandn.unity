using UnityEngine;

public class PhotoInGalleryScript : MonoBehaviour
{
    public int PhotoIndex { get; set; }
    public Animator photosAnimator;

    public void OpenPhoto()
    {
        GalleryManager.CurrentPhotoIndex = PhotoIndex;
        FindObjectOfType<ScreenManager>().OpenPanel(photosAnimator);
    }
}
