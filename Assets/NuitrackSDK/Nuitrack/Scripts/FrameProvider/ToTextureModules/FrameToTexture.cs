using UnityEngine;

public abstract class FrameToTexture : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    protected ComputeShader instanceShader;

    protected RenderTexture renderTexture;
    protected Texture2D texture2D;

    protected Rect rect;

    protected uint x, y, z;
    protected int kernelIndex;

    protected ulong lastTimeStamp;

    protected virtual void Awake()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
#if UNITY_EDITOR && !UNITY_STANDALONE
            Debug.LogError("Compute shaders are not supported for the Android platform in the editor.\n" +
                "A software conversion will be used (may cause performance issues)\n" +
                "Switch the platform to Standalone (this is not relevant for the assembled project).");
#else
            Debug.LogError("Compute shaders are not supported. A software conversion will be used (may cause performance issues).");
#endif

        }
    }

    protected void InitShader(string kernelName)
    {
        if (instanceShader != null)
            Destroy(instanceShader);

        instanceShader = Instantiate(computeShader);
        kernelIndex = instanceShader.FindKernel(kernelName);
        instanceShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);
    }

    protected void InitRenderTexture(int width, int height)
    {
        renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        instanceShader.SetTexture(kernelIndex, "Result", renderTexture);
    }

    /// <summary>
    /// Get the frame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <returns>Frame converted to RenderTexture</returns>
    public abstract RenderTexture GetRenderTexture();

    /// <summary>
    /// Get a frame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns>Frame converted to Texture2D</returns>
    public abstract Texture2D GetTexture2D();

    /// <summary>
    /// Convert Frame to Texture. 
    /// The method will select the most productive way to get the texture. 
    /// This can be either RenderTexture or Texture2D. 
    /// Use this method if you don't care about the texture type.
    /// </summary>
    /// <returns>Texture = (RenderTexture or Texture2D)</returns>
    public Texture GetTexture()
    {
        if (SystemInfo.supportsComputeShaders)
            return GetRenderTexture();
        else
            return GetTexture2D();
    }

    protected virtual void OnDestroy()
    {
        if (instanceShader != null)
            Destroy(instanceShader);

        if (renderTexture != null)
            Destroy(renderTexture);

        if (texture2D != null)
            Destroy(texture2D);
    }

    protected void CopyTexture(Texture2D source, ref RenderTexture dest)
    {
        if (dest == null || dest.width != source.width || dest.height != source.height)
            dest = new RenderTexture(source.width, source.height, 0);

        RenderTexture saveCameraRT = null;

        if (Camera.main != null)
        {
            saveCameraRT = Camera.main.targetTexture;
            Camera.main.targetTexture = null;
        }

        RenderTexture saveRT = RenderTexture.active;

        RenderTexture.active = dest;
        Graphics.Blit(source, dest);

        RenderTexture.active = saveRT;

        if (Camera.main != null)
            Camera.main.targetTexture = saveCameraRT;
    }

    protected void CopyTexture(RenderTexture source, ref Texture2D dest)
    {
        if (dest == null || dest.width != rect.width || dest.height != rect.height)
        {
            dest = new Texture2D(source.width, source.height, TextureFormat.ARGB32, false);
            rect = new Rect(0, 0, source.width, source.height);
        }

        RenderTexture saveRT = RenderTexture.active;

        RenderTexture.active = source;
        dest.ReadPixels(rect, 0, 0, false);
        dest.Apply();

        RenderTexture.active = saveRT;
    }
}
