using UnityEngine;

public abstract class FrameToTexture : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    protected ComputeShader instanceShader;

    protected RenderTexture renderTexture;
    protected Texture2D outRgbTexture;

    protected Rect rect;

    protected uint x, y, z;
    protected int kernelIndex;

    protected ulong lastTimeStamp;

    protected virtual void Awake()
    {
        if (!SystemInfo.supportsComputeShaders)
        {
#if UNITY_EDITOR && !UNITY_STANDALONE
            Debug.LogWarning("Compute shaders are not supported for the Android platform in the editor.\n" +
                "A software conversion will be used (may cause performance issues)\n" +
                "Switch the platform to Standalone (this is not relevant for the assembled project).");
#else
            Debug.LogWarning("Compute shaders are not supported. A software conversion will be used (may cause performance issues).");
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
    /// <returns></returns>
    public abstract RenderTexture GetRenderTexture();

    /// <summary>
    /// Get a frame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns></returns>
    public abstract Texture2D GetTexture2D();

    protected virtual void OnDestroy()
    {
        if (instanceShader != null)
            Destroy(instanceShader);

        if (renderTexture != null)
            Destroy(renderTexture);
    }

    protected void CopyTexture2DToRenderTexture()
    {
        if (renderTexture == null)
            renderTexture = new RenderTexture(outRgbTexture.width, outRgbTexture.height, 0);

        RenderTexture.active = renderTexture;
        Graphics.Blit(outRgbTexture, renderTexture);
    }

    protected void CopyRenderTextureToTexture2D()
    {
        RenderTexture.active = renderTexture;
        outRgbTexture.ReadPixels(rect, 0, 0, false);
        outRgbTexture.Apply();
    }
}
