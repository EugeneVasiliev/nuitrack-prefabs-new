using UnityEngine;
using FrameProviderModules;


[RequireComponent (typeof(RGBToTexture))]
[RequireComponent (typeof(DepthToTexture))]
[RequireComponent (typeof(SegmentToTexture))]
[RequireComponent(typeof(TextureUtils))]
public class FrameUtils : MonoBehaviour
{
    static FrameUtils instance;

    RGBToTexture rgbToTexture;
    DepthToTexture depthToTexture;
    SegmentToTexture segmentToTexture;

    TextureUtils textureUtils;

    public static FrameUtils Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("FrameUtils not found. Add a prefab FrameUtils to the scene.");

            return instance;
        }
    }

    public static RGBToTexture RGBToTexture
    {
        get
        {
            return Instance.rgbToTexture;
        }
    }

    public static DepthToTexture DepthToTexture
    {
        get
        {
            return Instance.depthToTexture;
        }
    }

    public static SegmentToTexture SegmentToTexture
    {
        get
        {
            return Instance.segmentToTexture;
        }
    }

    public static TextureUtils TextureUtils
    {
        get
        {
            return instance.textureUtils;
        }
    }

    void Awake()
    {
        instance = this;

        rgbToTexture = GetComponent<RGBToTexture>();
        depthToTexture = GetComponent<DepthToTexture>();
        segmentToTexture = GetComponent<SegmentToTexture>();
        textureUtils = GetComponent<TextureUtils>();
    }
}

public static class FrameOverloadUtils
{
    #region ColorFrame

    /// <summary>
    /// Get the ColorFrame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <returns>ColorFrame converted to RenderTexture</returns>
    public static RenderTexture ToRenderTexture(this nuitrack.ColorFrame frame)
    {
        return FrameUtils.RGBToTexture.GetRenderTexture(frame);
    }

    /// <summary>
    /// Get a ColorFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns>ColorFrame converted to RenderTexture</returns>
    public static Texture2D ToTexture2D(this nuitrack.ColorFrame frame)
    {
        return FrameUtils.RGBToTexture.GetTexture2D(frame);
    }

    /// <summary>
    /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T)"/> 
    /// </summary>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public static Texture ToTexture(this nuitrack.ColorFrame frame)
    {
        return FrameUtils.RGBToTexture.GetTexture(frame);
    }

    #endregion

    #region DepthFrame

    /// <summary>
    /// Get the DepthFrame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <returns>DepthFrame converted to RenderTexture</returns>
    public static RenderTexture ToRenderTexture(this nuitrack.DepthFrame frame)
    {
        return FrameUtils.DepthToTexture.GetRenderTexture(frame);
    }

    /// <summary>
    /// Get a DepthFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns>DepthFrame converted to Texture2D</returns>
    public static Texture2D ToTexture2D(this nuitrack.DepthFrame frame)
    {
        return FrameUtils.DepthToTexture.GetTexture2D(frame);
    }

    /// <summary>
    /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T)"/> 
    /// </summary>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public static Texture ToTexture(this nuitrack.DepthFrame frame)
    {
        return FrameUtils.DepthToTexture.GetTexture(frame);
    }

    #endregion

    #region UserFrame

    /// <summary>
    /// Get the UserFrame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <param name="userColors">Colors for user segments (optional).</param>
    /// <returns>UserFrame converted to RenderTexture</returns>
    public static RenderTexture ToRenderTexture(this nuitrack.UserFrame frame, Color[] userColors = null)
    {
        return FrameUtils.SegmentToTexture.GetRenderTexture(frame, userColors);
    }

    /// <summary>
    /// Get a UserFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <param name="userColors">Colors for user segments (optional).</param>
    /// <returns>UserFrame converted to Texture2D</returns>
    public static Texture2D ToTexture2D(this nuitrack.UserFrame frame, Color[] userColors = null)
    {
        return FrameUtils.SegmentToTexture.GetTexture2D(frame, userColors);
    }

    /// <summary>
    /// See the method description: <see cref="FrameToTexture{T, U}.GetTexture(T)"/> 
    /// </summary>
    /// <param name="userColors">Colors for user segments (optional)</param>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public static Texture ToTexture(this nuitrack.UserFrame frame, Color[] userColors = null)
    {
        return FrameUtils.SegmentToTexture.GetTexture(frame, userColors);
    }

    #endregion
}