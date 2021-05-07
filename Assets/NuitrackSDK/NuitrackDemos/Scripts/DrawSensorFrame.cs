using UnityEngine;

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
    [SerializeField] FrameType defaultFrameType = FrameType.Color;

    public void SwitchByIndex(int frameIndex)
    {
        if (frameIndex == 0) SelectFrame(FrameType.Color);
        if (frameIndex == 1) SelectFrame(FrameType.Depth);
        if (frameIndex == 2) SelectFrame(FrameType.User);
    }

    void Start()
    {
        SelectFrame(defaultFrameType);
    }

    void SelectFrame(FrameType frameType)
    {
        colorImage.SetActive(frameType == FrameType.Color);
        depthImage.SetActive(frameType == FrameType.Depth);
        userImage.SetActive(frameType == FrameType.User);
    }
}
