using System;

using UnityEngine;
using FrameProviderModules;

using CopyTextureSupport = UnityEngine.Rendering.CopyTextureSupport;

public class TextureUtils : MonoBehaviour
{
    [SerializeField] ComputeShader computeShader;
    ComputeShader instanceShader;

    public enum Operation
    {
        Cut = 0,
        ReverseCut,
        Mul,
        MixMask
    }

    ComputeShader ComputeShader
    {
        get
        {
            if (instanceShader == null)
                instanceShader = Instantiate(computeShader);

            return instanceShader;
        }
    }

    private void OnDestroy()
    {
        if (instanceShader != null)
        {
            Destroy(instanceShader);
            instanceShader = null;
        }
    }

    #region Join

    public void Join(Texture texture, Texture mask, ref RenderTexture renderTexture, Operation operation)
    {
        JoinTextures(texture, mask, operation.ToString(), ref renderTexture);
    }

    public void Join(FrameToTexture texture, FrameToTexture mask, ref RenderTexture renderTexture, Operation operation)
    {
        JoinTextures(texture.GetRenderTexture(), mask.GetRenderTexture(), operation.ToString(), ref renderTexture);
    }

    void JoinTextures(Texture texture, Texture mask, string kernelName, ref RenderTexture renderTexture)
    {
        if (texture == null && mask == null)
            return;

        int kernelIndex = ComputeShader.FindKernel(kernelName);
        uint x, y, z;
        instanceShader.GetKernelThreadGroupSizes(kernelIndex, out x, out y, out z);

        instanceShader.SetTexture(kernelIndex, "Texture", texture);
        instanceShader.SetTexture(kernelIndex, "Mask", mask);

        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        instanceShader.SetTexture(kernelIndex, "Output", renderTexture);

        instanceShader.Dispatch(kernelIndex, renderTexture.width / (int)x, renderTexture.height / (int)y, (int)z);
    }

    #endregion

    #region Copy

    bool CopyTextureSupportType(CopyTextureSupport textureSupport)
    {
        return (SystemInfo.copyTextureSupport & textureSupport) == textureSupport;
    }

    bool EqualsFormats<T>(Enum currentFormat)
    {
        return Enum.IsDefined(typeof(T), currentFormat.ToString());
    }

    T ConvertFormat<T>(Enum currentFormat)
    {
        return (T)Enum.Parse(typeof(T), currentFormat.ToString());
    }

    /// <summary>
    /// Copy Texture2D to RenderTexture.
    /// </summary>
    /// <param name="source">Source Texture2D</param>
    /// <param name="dest">Destination RenderTexture. Can be null. 
    /// If not null and the resolution or format does not match, the RenderTexture will be reinitialized.</param>
    /// <exception cref="Exception">If there is no format for RenderTexture corresponding to Texture2D.</exception>
    public void Copy(Texture2D source, ref RenderTexture dest)
    {
        if(!EqualsFormats<RenderTextureFormat>(source.format))
            throw new Exception(string.Format("Unable to copy Texture2D to RenderTexture. RenderTexture does not have the corresponding {0} format.", source.format));

        RenderTextureFormat textureFormat = ConvertFormat<RenderTextureFormat>(source.format);

        if (dest == null || dest.width != source.width || dest.height != source.height || dest.format != textureFormat)
            dest = new RenderTexture(source.width, source.height, 0, textureFormat);

        if (CopyTextureSupportType(CopyTextureSupport.TextureToRT))
            Graphics.CopyTexture(source, dest);
        else
        {
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
    }

    /// <summary>
    /// Copy RenderTexture to Texture2D.
    /// </summary>
    /// <param name="source">Source RenderTexture</param>
    /// <param name="dest">Destination Texture2D. Can be null.
    /// If not null and the resolution or format does not match, the Texture2D will be reinitialized.</param>
    public void Copy(RenderTexture source, ref Texture2D dest)
    {
        bool fastCopySupported = false;
        TextureFormat textureFormat;

        if (EqualsFormats<TextureFormat>(source.format))
        {
            fastCopySupported = true;
            textureFormat = ConvertFormat<TextureFormat>(source.format);
        }
        else
            textureFormat = TextureFormat.ARGB32;

        if (dest == null || dest.width != source.width || dest.height != source.height || dest.format != textureFormat)
            dest = new Texture2D(source.width, source.height, textureFormat, false);

        if (fastCopySupported && CopyTextureSupportType(CopyTextureSupport.RTToTexture))
            Graphics.CopyTexture(source, dest);
        else
        {
            Rect rect = new Rect(0, 0, source.width, source.height);

            RenderTexture saveRT = RenderTexture.active;

            RenderTexture.active = source;
            dest.ReadPixels(rect, 0, 0, false);
            dest.Apply();

            RenderTexture.active = saveRT;
        }
    }

    #endregion
  
}