using UnityEngine;

public class FrameProvider : MonoBehaviour
{
    static FrameProvider instance;

    RGBToTexture rgbToTexture;
    DepthToTexture depthToTexture;
    SegmentToTexture segmentToTexture;

    public static RGBToTexture ColorFrame
    {
        get
        {
            return instance.rgbToTexture;
        }
    }

    public static DepthToTexture DepthFrame
    {
        get
        {
            return instance.depthToTexture;
        }
    }

    public static SegmentToTexture UserFrame
    {
        get
        {
            return instance.segmentToTexture;
        }
    }

    private void Awake()
    {
        instance = this;

        rgbToTexture = GetComponent<RGBToTexture>();
        depthToTexture = GetComponent<DepthToTexture>();
        segmentToTexture = GetComponent<SegmentToTexture>();
    }

}
