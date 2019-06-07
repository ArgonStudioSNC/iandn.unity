using UnityEngine;

public class SafeArea : MonoBehaviour
{
    RectTransform Panel;
    Rect LastSafeArea = new Rect(0, 0, 0, 0);

    protected void Awake()
    {
        Panel = GetComponent<RectTransform>();
        Refresh();
    }

    protected void Update()
    {
        Refresh();
    }

    protected void Refresh()
    {
        Rect safeArea = GetSafeArea();

        if (safeArea != LastSafeArea)
            ApplySafeArea(safeArea);
    }

    private Rect GetSafeArea()
    {
        return Screen.safeArea;
    }

    private void ApplySafeArea(Rect r)
    {
        LastSafeArea = r;

        // Convert safe area rectangle from absolute pixels to normalized anchor coordinates
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;
        anchorMin.x /= Screen.width;
        //anchorMin.y /= Screen.height;
        anchorMin.y = 0; // we don't want a bottom safe zone for I&N app
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        Panel.anchorMin = anchorMin;
        Panel.anchorMax = anchorMax;

        Debug.LogFormat("New safe area applied to {0}: x={1}, y={2}, w={3}, h={4} on full extents w={5}, h={6}",
            name, r.x, r.y, r.width, r.height, Screen.width, Screen.height);
    }
}