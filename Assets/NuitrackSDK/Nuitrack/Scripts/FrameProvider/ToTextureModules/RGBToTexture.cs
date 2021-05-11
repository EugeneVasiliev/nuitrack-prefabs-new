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
        if (frame.Timestamp == lastTimeStamp && outRgbTexture != null)
            return outRgbTexture;
        else
        {
            outRgbTexture = frame.ToTexture2D();
            lastTimeStamp = frame.Timestamp;

            return outRgbTexture;
        }       
    }

    RenderTexture GetGPUTexture(nuitrack.ColorFrame frame)
    {
        if (frame.Timestamp == lastTimeStamp && outRgbTexture != null)
            return renderTexture;
        else
        {
            lastTimeStamp = frame.Timestamp;

            if (outRgbTexture == null)
            {
                outRgbTexture = new Texture2D(frame.Cols, frame.Rows, TextureFormat.RGB24, false);
                rect = new Rect(0, 0, frame.Cols, frame.Rows);
            }

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

    public override RenderTexture GetRenderTexture()
    {
        if (SourceFrame == null)
            return null;

        if (SystemInfo.supportsComputeShaders)
            return GetGPUTexture(SourceFrame);
        else
        {
            outRgbTexture = GetCPUTexture(SourceFrame);
            CopyTexture2DToRenderTexture();

            return renderTexture;
        }
    }

    public override Texture2D GetTexture2D()
    {
        if (SourceFrame == null)
            return null;

        if (!SystemInfo.supportsComputeShaders)
            return GetCPUTexture(SourceFrame);
        else
        {
            renderTexture = GetGPUTexture(SourceFrame);
            CopyRenderTextureToTexture2D();
            return outRgbTexture;
        }
    }
}
