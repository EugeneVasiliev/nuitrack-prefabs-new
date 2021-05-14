using UnityEngine;
using UnityEngine.UI;

public enum FrameType
{
    Color,
    Depth,
    User,
}

public class DrawSensorFrame : MonoBehaviour
{
    [SerializeField] GameObject colorImage;
    [SerializeField] GameObject depthImage;
    [SerializeField] GameObject userImage;
    [SerializeField] GameObject segmentOverlay;
    [SerializeField] FrameType defaultFrameType = FrameType.Color;

    [SerializeField] RectTransform panel;
    [SerializeField] int windowPercent = 20;
    [SerializeField] bool fullscreenDefault = true;

    bool isFullscreen;

    public void SwitchByIndex(int frameIndex)
    {
        if (frameIndex == 0) SelectFrame(FrameType.Color);
        if (frameIndex == 1) SelectFrame(FrameType.Depth);
        if (frameIndex == 2) SelectFrame(FrameType.User);
    }

    void Start()
    {
        SelectFrame(defaultFrameType);
        isFullscreen = fullscreenDefault;
        SwitchFullscreen();
    }

    void SelectFrame(FrameType frameType)
    {
        colorImage.SetActive(frameType == FrameType.Color);
        depthImage.SetActive(frameType == FrameType.Depth);
        userImage.SetActive(frameType == FrameType.User);
    }

    public void SwitchSegmentOverlay()
    {
        segmentOverlay.SetActive(!segmentOverlay.activeSelf);
    }

    public void SwitchFullscreen()
    {
        isFullscreen = !isFullscreen;

        if (isFullscreen)
            panel.localScale = new Vector3(1.0f/100*windowPercent, 1.0f / 100 * windowPercent, 1.0f);
        else
            panel.localScale = new Vector3(1, 1, 1);
    }
}
