using UnityEngine;
using FrameProviderModules;


[RequireComponent (typeof(RGBToTexture))]
[RequireComponent (typeof(DepthToTexture))]
[RequireComponent (typeof(SegmentToTexture))]
[RequireComponent(typeof(TextureUtils))]

public class FrameProvider : MonoBehaviour
{
    static FrameProvider instance;

    RGBToTexture rgbToTexture;
    DepthToTexture depthToTexture;
    SegmentToTexture segmentToTexture;
    TextureUtils frameUtils;

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

    public static TextureUtils FrameUtils
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
        frameUtils = GetComponent<TextureUtils>();
    }
}

public static class FrameUtils
{
    #region ColorFrame

    /// <summary>
    /// Get the ColorFrame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <returns>ColorFrame converted to RenderTexture</returns>
    public static RenderTexture ToRenderTexture(this nuitrack.ColorFrame frame)
    {
        return FrameProvider.ColorFrame.GetRenderTexture();
    }

    /// <summary>
    /// Get a ColorFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns>ColorFrame converted to RenderTexture</returns>
    public static Texture2D ToTexture2D(this nuitrack.ColorFrame frame)
    {
        return FrameProvider.ColorFrame.GetTexture2D();
    }

    /// <summary>
    /// See the method description: <see cref="FrameToTexture.GetTexture"/> 
    /// </summary>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public static Texture ToTexture(this nuitrack.ColorFrame frame)
    {
        return FrameProvider.ColorFrame.GetTexture();
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
        return FrameProvider.DepthFrame.GetRenderTexture();
    }

    /// <summary>
    /// Get a DepthFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns>DepthFrame converted to Texture2D</returns>
    public static Texture2D ToTexture2D(this nuitrack.DepthFrame frame)
    {
        return FrameProvider.DepthFrame.GetTexture2D();
    }

    /// <summary>
    /// See the method description: <see cref="FrameToTexture.GetTexture"/> 
    /// </summary>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public static Texture ToTexture(this nuitrack.DepthFrame frame)
    {
        return FrameProvider.DepthFrame.GetTexture();
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
        return FrameProvider.UserFrame.GetRenderTexture(userColors);
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
        return FrameProvider.UserFrame.GetTexture2D(userColors);
    }

    /// <summary>
    /// See the method description: <see cref="FrameToTexture.GetTexture"/> 
    /// </summary>
    /// <param name="userColors">Colors for user segments (optional)</param>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public static Texture ToTexture(this nuitrack.UserFrame frame, Color[] userColors = null)
    {
        return FrameProvider.UserFrame.GetTexture(userColors);
    }

  

    #endregion
}