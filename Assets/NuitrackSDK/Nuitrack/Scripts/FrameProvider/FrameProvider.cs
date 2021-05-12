using UnityEngine;

[RequireComponent (typeof(RGBToTexture))]
[RequireComponent (typeof(DepthToTexture))]
[RequireComponent (typeof(SegmentToTexture))]
[RequireComponent(typeof(FrameUtils))]

public class FrameProvider : MonoBehaviour
{
    static FrameProvider instance;

    RGBToTexture rgbToTexture;
    DepthToTexture depthToTexture;
    SegmentToTexture segmentToTexture;
    FrameUtils frameUtils;

    public static FrameProvider Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("FrameProvider not found. Add a prefab FrameProvider to the scene.");

            return instance;
        }
    }

    public static RGBToTexture ColorFrame
    {
        get
        {
            return Instance.rgbToTexture;
        }
    }

    public static DepthToTexture DepthFrame
    {
        get
        {
            return Instance.depthToTexture;
        }
    }

    public static SegmentToTexture UserFrame
    {
        get
        {
            return Instance.segmentToTexture;
        }
    }

    public static FrameUtils FrameUtils
    {
        get
        {
            return instance.frameUtils;
        }
    }

    void Awake()
    {
        instance = this;

        rgbToTexture = GetComponent<RGBToTexture>();
        depthToTexture = GetComponent<DepthToTexture>();
        segmentToTexture = GetComponent<SegmentToTexture>();
        frameUtils = GetComponent<FrameUtils>();
    }

}
