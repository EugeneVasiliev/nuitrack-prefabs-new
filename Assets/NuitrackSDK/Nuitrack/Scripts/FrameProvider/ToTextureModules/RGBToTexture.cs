/*
 * This script converts source data of nuitrack.ColorFrame to RenderTexture using the GPU,
 * which is several times faster than CPU conversions.
 * 
 * Learn more about supported platforms and the graphics API: https://docs.unity3d.com/Manual/class-ComputeShader.html
*/


using UnityEngine;

public class RGBToTexture : FrameToTexture
{
    Texture2D dstRgbTexture2D;

    public nuitrack.ColorFrame SourceFrame
    {
        get
        {
            return NuitrackManager.ColorFrame;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Destroy(dstRgbTexture2D);
    }

    Texture2D GetCPUTexture(nuitrack.ColorFrame frame)
    {
        if (frame.Timestamp == lastTimeStamp && texture2D != null)
            return texture2D;
        else
        {
            texture2D = frame.ToTexture2D();
            lastTimeStamp = frame.Timestamp;

            return texture2D;
        }       
    }

    RenderTexture GetGPUTexture(nuitrack.ColorFrame frame)
    {
        if (frame.Timestamp == lastTimeStamp && renderTexture != null)
            return renderTexture;
        else
        {
            lastTimeStamp = frame.Timestamp;

            if (instanceShader == null)
                InitShader("RGB2BGR");

            if (renderTexture == null)
            {
                dstRgbTexture2D = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
                instanceShader.SetTexture(kernelIndex, "Texture", dstRgbTexture2D);

                InitRenderTexture(frame.Cols, frame.Rows);
            }

            dstRgbTexture2D.LoadRawTextureData(frame.Data, frame.DataSize);
            dstRgbTexture2D.Apply();

            instanceShader.Dispatch(kernelIndex, dstRgbTexture2D.width / (int)x, dstRgbTexture2D.height / (int)y, (int)z);

            return renderTexture;
        }
    }

    /// <summary>
    /// Get the ColorFrame as a RenderTexture. 
    /// Recommended method for platforms with ComputeShader support.
    /// </summary>
    /// <returns>ColorFrame converted to RenderTexture</returns>
    public override RenderTexture GetRenderTexture()
    {
        if (SourceFrame == null)
            return null;

        if (SystemInfo.supportsComputeShaders)
            return GetGPUTexture(SourceFrame);
        else
        {
            texture2D = GetCPUTexture(SourceFrame);
            CopyTexture(texture2D, ref renderTexture);

            return renderTexture;
        }
    }

    /// <summary>
    /// Get a ColorFrame in the form of Texture2D. 
    /// For platforms with ComputeShader support, it may be slower than GetRenderTexture. 
    /// If possible, use GetRenderTexture.
    /// </summary>
    /// <returns>ColorFrame converted to Texture2D</returns>
    public override Texture2D GetTexture2D()
    {
        if (SourceFrame == null)
            return null;

        if (!SystemInfo.supportsComputeShaders)
            return GetCPUTexture(SourceFrame);
        else
        {
            renderTexture = GetGPUTexture(SourceFrame);
            CopyTexture(renderTexture, ref texture2D);
            return texture2D;
        }
    }
}
